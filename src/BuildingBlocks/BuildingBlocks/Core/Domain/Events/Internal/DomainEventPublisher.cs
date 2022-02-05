using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Events.External;
using BuildingBlocks.Core.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Core.Domain.Events.Internal;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IDomainNotificationEventPublisher _domainNotificationEventPublisher;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public DomainEventPublisher(
        IMediator mediator,
        IIntegrationEventPublisher integrationEventPublisher,
        IDomainNotificationEventPublisher domainNotificationEventPublisher,
        IServiceProvider serviceProvider)
    {
        _domainNotificationEventPublisher =
            Guard.Against.Null(domainNotificationEventPublisher, nameof(domainNotificationEventPublisher));
        _integrationEventPublisher = Guard.Against.Null(integrationEventPublisher, nameof(integrationEventPublisher));
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _serviceProvider = Guard.Against.Null(serviceProvider, nameof(serviceProvider));
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent[] domainEvents, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainEvents, nameof(domainEvents));

        if (domainEvents.Any() == false)
            return;

        var domainEventContext = _serviceProvider.GetRequiredService<IDomainEventContext>();

        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/

        // Dispatch our domain events before commit
        var eventsToDispatch = domainEvents.ToList();

        if (eventsToDispatch.Any() == false)
        {
            var aggregatesTuple = domainEventContext.GetAggregateDomainEvents();
            eventsToDispatch = aggregatesTuple.SelectMany(x => x.DomainEvents).ToList();
        }

        await _mediator.DispatchDomainEventAsync(eventsToDispatch, cancellationToken: cancellationToken);

        // Save wrapped integration and notification events to outbox for further processing after commit
        var wrappedNotificationEvents = eventsToDispatch.GetWrappedDomainNotificationEvents().ToArray();
        await _domainNotificationEventPublisher.PublishAsync(wrappedNotificationEvents.ToArray(), cancellationToken);

        var wrappedIntegrationEvents = eventsToDispatch.GetWrappedIntegrationEvents().ToArray();
        await _integrationEventPublisher.PublishAsync(wrappedIntegrationEvents.ToArray(), cancellationToken);

        // Save event mapper events into outbox for further processing after commit
        IEventMapper? eventMapper = _serviceProvider.GetService<IEventMapper>();
        IIntegrationEventMapper? integrationEventMapper = _serviceProvider.GetService<IIntegrationEventMapper>();
        IIDomainNotificationEventMapper? notificationMapper =
            _serviceProvider.GetService<IIDomainNotificationEventMapper>();

        var integrationEvents = eventMapper?.MapToIntegrationEvents(eventsToDispatch) ??
                                integrationEventMapper?.MapToIntegrationEvents(eventsToDispatch);

        integrationEvents = integrationEvents?.Where(x => x is not null).ToList();

        if (integrationEvents is not null && integrationEvents.Any())
        {
            await _integrationEventPublisher.PublishAsync(integrationEvents.ToArray()!, cancellationToken);
        }

        var notificationEvents =
            eventMapper?.MapToDomainNotificationEvents(eventsToDispatch) ??
            notificationMapper?.MapToDomainNotificationEvents(eventsToDispatch);

        notificationEvents = notificationEvents?.Where(x => x is not null).ToList();

        if (notificationEvents is not null && notificationEvents.Any())
        {
            await _domainNotificationEventPublisher.PublishAsync(notificationEvents.ToArray()!, cancellationToken);
        }
    }
}