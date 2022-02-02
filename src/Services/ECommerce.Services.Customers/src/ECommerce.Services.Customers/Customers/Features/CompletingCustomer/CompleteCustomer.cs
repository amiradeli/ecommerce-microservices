using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.ValueObjects;
using BuildingBlocks.CQRS.Command;
using BuildingBlocks.Exception;
using ECommerce.Services.Customers.Customers.Exceptions;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Shared.Extensions;

namespace ECommerce.Services.Customers.Customers.Features.CompletingCustomer;

public record CompleteCustomer(
    long CustomerId,
    string PhoneNumber,
    DateTime? BirthDate = null,
    string? Country = null,
    string? City = null,
    string? DetailAddress = null,
    string? Nationality = null) : ICommand, ITxRequest;

internal class CompleteCustomerValidator : AbstractValidator<CompleteCustomer>
{
    public CompleteCustomerValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .GreaterThan(0).WithMessage("CustomerId must be greater than 0");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+[0-9]{11}$")
            .WithMessage("PhoneNumber must be a valid phone number");
    }
}

internal class CompleteCustomerHandler : ICommandHandler<CompleteCustomer>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<CompleteCustomerHandler> _logger;

    public CompleteCustomerHandler(CustomersDbContext customersDbContext, ILogger<CompleteCustomerHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(CompleteCustomer command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var customer = await _customersDbContext.FindCustomerByIdAsync(command.CustomerId, cancellationToken);

        Guard.Against.NotFound(customer, new CustomerNotFoundException(command.CustomerId));

        if (await _customersDbContext.Customers
                .AnyAsync(x => x.PhoneNumber == command.PhoneNumber, cancellationToken: cancellationToken))
        {
            throw new CustomerAlreadyExistsException(command.PhoneNumber);
        }

        customer!.Complete(
            new PhoneNumber(command.PhoneNumber),
            DateTime.Now,
            Address.Create(command.Country, command.City, command.DetailAddress),
            command.Nationality,
            command.BirthDate);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed a customer with ID: '{CustomerId}'", command.CustomerId);

        return Unit.Value;
    }
}