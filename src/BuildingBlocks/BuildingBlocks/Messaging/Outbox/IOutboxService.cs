using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Events.External;

namespace BuildingBlocks.Messaging.Outbox;

public interface IOutboxService
{
    Task<IEnumerable<OutboxMessage>> GetAllUnsentOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<OutboxMessage>> GetAllOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default);

    Task CleanProcessedAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task SaveAsync(IDomainNotificationEvent domainNotificationEvent, CancellationToken cancellationToken = default);
    Task PublishUnsentOutboxMessagesAsync(CancellationToken cancellationToken = default);
}
