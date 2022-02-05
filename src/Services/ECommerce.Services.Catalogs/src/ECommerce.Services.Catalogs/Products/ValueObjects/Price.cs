using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Model;
using BuildingBlocks.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

public class Price : ValueObject
{
    public decimal Value { get; private set; }

    public Price? Null => null;

    public static Price Create(decimal value)
    {
        return new Price
        {
            Value = Guard.Against.NegativeOrZero(
                value,
                new ProductDomainException("The catalog item price cannot have negative or zero value."))
        };
    }

    public static implicit operator Price(decimal value) => Create(value);

    public static implicit operator decimal(Price value) =>
        Guard.Against.Null(value.Value, new ProductDomainException("Price can't be null."));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}