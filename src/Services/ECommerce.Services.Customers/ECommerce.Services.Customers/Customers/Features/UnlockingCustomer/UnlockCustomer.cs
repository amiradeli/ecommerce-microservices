using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Shared.Extensions;
using MicroBootstrap.Abstractions.CQRS.Command;

namespace ECommerce.Services.Customers.Customers.Features.UnlockingCustomer;

public record UnlockCustomer(long CustomerId, string? Notes = null) : ITxCommand;

internal class UnlockCustomerValidator : AbstractValidator<UnlockCustomer>
{
    public UnlockCustomerValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}

internal class UnlockCustomerHandler : ICommandHandler<UnlockCustomer>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<UnlockCustomerHandler> _logger;

    public UnlockCustomerHandler(CustomersDbContext customersDbContext, ILogger<UnlockCustomerHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(UnlockCustomer command, CancellationToken cancellationToken)
    {
        var customer = await _customersDbContext.FindCustomerByIdAsync(command.CustomerId);
        if (customer is null)
        {
            throw new CustomerNotFoundException(command.CustomerId);
        }

        customer.Unlock(command.Notes);
        await _customersDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Unlocked a customer with ID: '{CustomerId}'", command.CustomerId);

        return Unit.Value;
    }
}
