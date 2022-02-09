using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.CQRS.Command;
using BuildingBlocks.Exception;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscriptionReadModel;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Shared.Extensions;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;

public record RestockSubscriptionCreated(RestockSubscription RestockSubscription) : DomainEvent;

internal class RestockSubscriptionCreatedHandler : IDomainEventHandler<RestockSubscriptionCreated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionCreatedHandler(ICommandProcessor commandProcessor, CustomersDbContext customersDbContext)
    {
        _commandProcessor = commandProcessor;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockSubscriptionCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var customer = await _customersDbContext.FindCustomerByIdAsync(notification.RestockSubscription.CustomerId);

        Guard.Against.NotFound(
            customer,
            new CustomerNotFoundException(notification.RestockSubscription.CustomerId));

        await _commandProcessor.SendAsync(
            new CreateRestockSubscriptionReadModels(
                customer!.Id.Value,
                customer.Name.FullName,
                notification.RestockSubscription.ProductInformation.Id,
                notification.RestockSubscription.ProductInformation.Name,
                notification.RestockSubscription.Processed,
                notification.RestockSubscription.ProcessedTime),
            cancellationToken);
    }
}