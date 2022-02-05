using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Model;

namespace ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

public class RestockSubscriptionId : AggregateId<long>
{
    public RestockSubscriptionId(long value) : base(value)
    {
    }

    public static implicit operator long(RestockSubscriptionId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator RestockSubscriptionId(long id) => new(id);
}