using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Model;

namespace ECommerce.Services.Customers.Customers.ValueObjects;

public class CustomerId : AggregateId
{
    public CustomerId(long value) : base(value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public static implicit operator long(CustomerId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator CustomerId(long id) => new(id);
}