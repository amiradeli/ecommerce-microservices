using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Core.Persistence.EfCore;

public class EfDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IDomainEventContext _domainEventContext;
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;

    public EfDomainEventAccessor(
        IDomainEventContext domainEventContext,
        IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        _domainEventContext = domainEventContext;
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents
    {
        get
        {
            _ = _aggregatesDomainEventsStore.GetAllUncommittedEvents();

            // Or
            return _domainEventContext.GetAllUncommittedEvents();
        }
    }
}
