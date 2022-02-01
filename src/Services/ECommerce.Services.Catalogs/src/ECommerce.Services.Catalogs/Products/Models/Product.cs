using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Model;
using BuildingBlocks.Exception;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.Events.Domain;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.Events;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductPrice;
using ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.Events;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.Events.Domain;
using ECommerce.Services.Catalogs.Products.Models.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;
using static BuildingBlocks.Core.Domain.Events.Internal.DomainEvents;
using Size = ECommerce.Services.Catalogs.Products.Models.ValueObjects.Size;

namespace ECommerce.Services.Catalogs.Products.Models;

// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://enterprisecraftsmanship.com/posts/link-to-an-aggregate-reference-or-id/
// https://ardalis.com/avoid-collections-as-properties/?utm_sq=grcpqjyka3
public class Product : AggregateRoot<ProductId>
{
    private readonly List<ProductImage> _images = new();

    public Name Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Price Price { get; private set; } = null!;
    public ProductColor Color { get; private set; }
    public ProductStatus ProductStatus { get; private set; }
    public CategoryId CategoryId { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public SupplierId SupplierId { get; private set; } = null!;
    public Supplier Supplier { get; private set; } = null!;
    public BrandId BrandId { get; private set; } = null!;
    public Brand Brand { get; private set; } = null!;
    public Size Size { get; private set; } = null!;
    public Stock Stock { get; set; } = null!;
    public Dimensions Dimensions { get; private set; } = null!;
    public IReadOnlyList<ProductImage> Images => _images;


    public static Product Create(
        ProductId id,
        Name name,
        Stock stock,
        ProductStatus status,
        Dimensions dimensions,
        Size size,
        ProductColor color,
        string? description,
        Price price,
        Category? category,
        Supplier? supplier,
        Brand? brand,
        IList<ProductImage>? images = null)
    {
        var product = new Product
        {
            Id = Guard.Against.Null(id, new ProductDomainException("Product id can not be null")),
            Stock = Guard.Against.Null(stock, new ProductDomainException("Product stock can not be null"))
        };

        product.ChangeName(name);
        product.ChangeSize(size);
        product.ChangeDescription(description);
        product.ChangePrice(price);
        product.AddProductImages(images);
        product.ChangeStatus(status);
        product.ChangeColor(color);
        product.ChangeDimensions(dimensions);
        product.ChangeCategory(category);
        product.ChangeBrand(brand);
        product.ChangeSupplier(supplier);

        product.AddDomainEvent(new ProductCreated(product));

        return product;
    }

    public void ChangeStatus(ProductStatus status)
    {
        ProductStatus = status;
    }

    public void ChangeDimensions(Dimensions dimensions)
    {
        Guard.Against.Null(dimensions, new ProductDomainException("Dimensions cannot be null."));

        Dimensions = dimensions;
    }

    public void ChangeSize(Size size)
    {
        Guard.Against.Null(size, new ProductDomainException("Size cannot be null."));

        Size = size;
    }

    public void ChangeColor(ProductColor color)
    {
        Color = color;
    }

    /// <summary>
    /// Sets catalog item name.
    /// </summary>
    /// <param name="name">The name to be changed.</param>
    public void ChangeName(Name name)
    {
        Guard.Against.Null(name, new ProductDomainException("Product name cannot be null."));

        Name = name;
    }

    /// <summary>
    /// Sets catalog item description.
    /// </summary>
    /// <param name="description">The description to be changed.</param>
    public void ChangeDescription(string? description)
    {
        Description = description;
    }

    /// <summary>
    /// Sets catalog item price.
    /// </summary>
    /// <remarks>
    /// Raise a <see cref="ProductPriceChanged"/>.
    /// </remarks>
    /// <param name="price">The price to be changed.</param>
    public void ChangePrice(Price price)
    {
        Guard.Against.Null(price, new ProductDomainException("Price cannot be null."));

        if (Price == price)
            return;

        Price = price;

        AddDomainEvent(new ProductPriceChanged(price));
    }

    /// <summary>
    /// Decrements the quantity of a particular item in inventory and ensures the restockThreshold hasn't
    /// been breached. If so, a RestockRequest is generated in CheckThreshold.
    /// </summary>
    /// <param name="quantity">The number of items to debit.</param>
    /// <returns>int: Returns the number actually removed from stock. </returns>
    public int DebitStock(int quantity)
    {
        if (HasStock(quantity) == false)
        {
            throw new InsufficientStockException($"Empty stock, product item {Name} is sold out");
        }

        if (quantity < 0) quantity *= -1;

        int removed = Math.Min(quantity, Stock.Available);

        Stock = new Stock(Stock.Available - removed, Stock.RestockThreshold, Stock.MaxStockThreshold);

        if (Stock.Available <= Stock.RestockThreshold)
        {
            AddDomainEvent(new ProductRestockThresholdReachedEvent(Stock.Available, quantity));
        }

        AddDomainEvent(new ProductStockDebited(Stock.Available, quantity));

        return removed;
    }

    /// <summary>
    /// Increments the quantity of a particular item in inventory.
    /// </summary>
    /// <returns>int: Returns the quantity that has been added to stock.</returns>
    /// <param name="quantity">The number of items to Replenish.</param>
    public Stock ReplenishStock(int quantity)
    {
        // we don't have enough space in the inventory
        if (Stock.Available + quantity > Stock.MaxStockThreshold)
        {
            throw new MaxStockThresholdReachedException(
                $"Max stock threshold has been reached. Max stock threshold is {Stock.MaxStockThreshold}");
        }

        Stock = new Stock(Stock.Available + quantity, Stock.RestockThreshold, Stock.MaxStockThreshold);

        AddDomainEvent(new ProductStockReplenished(Stock.Available, quantity));

        return Stock;
    }

    public Stock ChangeMaxStockThreshold(int maxStockThreshold)
    {
        Guard.Against.NegativeOrZero(maxStockThreshold, nameof(maxStockThreshold));

        Stock = new Stock(Stock.Available, Stock.RestockThreshold, maxStockThreshold);

        AddDomainEvent(new MaxThresholdChanged(maxStockThreshold));

        return Stock;
    }

    public Stock ChangeRestockThreshold(int restockThreshold)
    {
        Guard.Against.NegativeOrZero(restockThreshold, nameof(restockThreshold));

        Stock = new Stock(Stock.Available, restockThreshold, Stock.MaxStockThreshold);

        AddDomainEvent(new RestockThresholdChanged(restockThreshold));

        return Stock;
    }

    public bool HasStock(int quantity)
    {
        return Stock.Available >= quantity;
    }

    public void Activate() => ChangeStatus(ProductStatus.Available);

    public void DeActive() => ChangeStatus(ProductStatus.Unavailable);

    /// <summary>
    /// Sets category.
    /// </summary>
    /// <param name="category">The category to be changed.</param>
    public void ChangeCategory(Category? category)
    {
        Guard.Against.Null(category, nameof(category));
        Guard.Against.NullOrEmpty(category.Code, nameof(category.Code));
        Guard.Against.NullOrEmpty(category.Name, nameof(category.Name));
        Guard.Against.NegativeOrZero(category.Id, nameof(category.Id));

        // raising domain event immediately for checking some validation rule with some dependencies such as database
        RaiseDomainEvent(new ChangingProductCategory(category.Id));

        Category = category;
        CategoryId = category.Id;

        // add event to domain events list that will be raise during commiting transaction
        AddDomainEvent(new ProductCategoryChanged(category.Id, Id));
    }

    /// <summary>
    /// Sets supplier.
    /// </summary>
    /// <param name="supplier">The supplier to be changed.</param>
    public void ChangeSupplier(Supplier? supplier)
    {
        Guard.Against.Null(supplier, nameof(supplier));
        Guard.Against.NullOrEmpty(supplier.Name, nameof(supplier.Name));
        Guard.Against.NegativeOrZero(supplier.Id, nameof(supplier.Id));

        RaiseDomainEvent(new ChangingProductSupplier(supplier.Id));

        Supplier = supplier;
        SupplierId = supplier.Id;

        AddDomainEvent(new ProductSupplierChanged(supplier.Id, Id));
    }

    /// <summary>
    ///  Sets brand.
    /// </summary>
    /// <param name="brand">The brand to be changed.</param>
    public void ChangeBrand(Brand? brand)
    {
        Guard.Against.Null(brand, nameof(brand));
        Guard.Against.NullOrEmpty(brand.Name, nameof(brand.Name));
        Guard.Against.NegativeOrZero(brand.Id, nameof(brand.Id));

        RaiseDomainEvent(new ChangingProductBrand(brand.Id));

        Brand = brand;
        BrandId = brand.Id;

        AddDomainEvent(new ProductBrandChanged(brand.Id, Id));
    }

    public void AddProductImages(IList<ProductImage>? productImages)
    {
        Guard.Against.Null(productImages, nameof(productImages));

        _images.AddRange(productImages);
    }
}
