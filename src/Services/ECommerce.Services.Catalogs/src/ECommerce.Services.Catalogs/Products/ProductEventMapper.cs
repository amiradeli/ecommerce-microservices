using BuildingBlocks.Core.Domain.Events;
using BuildingBlocks.Core.Domain.Events.External;
using BuildingBlocks.Core.Domain.Events.Internal;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.Events.Notification;
using ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.Events.Domain;
using ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.Events.Domain;

namespace ECommerce.Services.Catalogs.Products;

public class ProductEventMapper : IEventMapper
{
    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductCreated e =>
                new Features.CreatingProduct.Events.Integration.ProductCreated(
                    e.Product.Id,
                    e.Product.Name,
                    e.Product.CategoryId,
                    e.Product.Category.Name,
                    e.Product.Stock.Available),
            ProductStockDebited e => new
                Features.DebitingProductStock.Events.Integration.ProductStockDebited(
                    e.NewStock, e.DebitedQuantity),
            ProductStockReplenished e => new
                Features.ReplenishingProductStock.Events.Integration.ProductStockReplenished(
                    e.NewStock, e.ReplenishedQuantity),
            _ => null
        };
    }

    public IDomainNotificationEvent? MapToDomainNotificationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductCreated e => new ProductCreatedNotification(e),
            _ => null
        };
    }

    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList().AsReadOnly();
    }

    public IReadOnlyList<IDomainNotificationEvent?> MapToDomainNotificationEvents(
        IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToDomainNotificationEvent).ToList().AsReadOnly();
    }
}
