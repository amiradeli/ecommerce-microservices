using Ardalis.GuardClauses;
using AutoMapper;
using ECommerce.Services.Customers.Customers.Features.UpdatingMongoCustomerReadsModel;
using ECommerce.Services.Customers.Customers.Models;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.CQRS.Command;
using MicroBootstrap.Core.Domain.Events.Internal;

namespace ECommerce.Services.Customers.Customers.Features.CompletingCustomer.Events.Domain;

public record CustomerCompleted(Customer Customer) : DomainEvent;

internal class CustomerCompletedHandler : IDomainEventHandler<CustomerCompleted>
{
    private readonly IMapper _mapper;
    private readonly ICommandProcessor _commandProcessor;

    public CustomerCompletedHandler(IMapper mapper, ICommandProcessor commandProcessor)
    {
        _mapper = mapper;
        _commandProcessor = commandProcessor;
    }

    public Task Handle(CustomerCompleted notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var command = _mapper.Map<UpdateMongoCustomerReadsModel>(notification.Customer);

        return _commandProcessor.ScheduleAsync(command, cancellationToken);
    }
}
