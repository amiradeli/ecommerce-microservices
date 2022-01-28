using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Events.Internal;
using ECommerce.Services.Catalogs.Shared.Core.Contracts;
using ECommerce.Services.Catalogs.Shared.Infrastructure.Extensions;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier.Events;

public record ChangingProductSupplier(long SupplierId) : DomainEvent;

internal class ChangingSupplierValidationHandler :
    IDomainEventHandler<ChangingProductSupplier>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ChangingSupplierValidationHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task Handle(ChangingProductSupplier notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));
        Guard.Against.NegativeOrZero(notification.SupplierId, nameof(notification.SupplierId));
        Guard.Against.ExistsSupplier(
            await _catalogDbContext.SupplierExistsAsync(notification.SupplierId, cancellationToken: cancellationToken),
            notification.SupplierId);
    }
}