using Ardalis.GuardClauses;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using MicroBootstrap.Abstractions.CQRS.Command;
using MicroBootstrap.Core.Exception;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;

public record DeleteRestockSubscription(long Id) : ITxCommand;

internal class DeleteRestockSubscriptionValidator : AbstractValidator<DeleteRestockSubscription>
{
    public DeleteRestockSubscriptionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class DeleteRestockSubscriptionHandler : ICommandHandler<DeleteRestockSubscription>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<DeleteRestockSubscriptionHandler> _logger;

    public DeleteRestockSubscriptionHandler(
        CustomersDbContext customersDbContext,
        ILogger<DeleteRestockSubscriptionHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteRestockSubscription command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var exists = await _customersDbContext.RestockSubscriptions.IgnoreAutoIncludes()
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        Guard.Against.NotFound(exists, new RestockSubscriptionNotFoundException(command.Id));

        // for raising a deleted domain event
        exists!.Delete();

        _customersDbContext.Entry(exists).State = EntityState.Deleted;
        _customersDbContext.Entry(exists.ProductInformation).State = EntityState.Unchanged;

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RestockSubscription with id '{Id} removed.'", command.Id);

        return Unit.Value;
    }
}
