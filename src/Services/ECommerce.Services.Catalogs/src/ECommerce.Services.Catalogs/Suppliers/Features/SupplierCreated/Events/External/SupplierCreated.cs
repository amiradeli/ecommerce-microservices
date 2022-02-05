using BuildingBlocks.Core.Domain.Events;
using BuildingBlocks.Core.Domain.Events.External;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierCreated.Events.External;

public record SupplierCreated(long Id, string Name) : IntegrationEvent;


public class SupplierCreatedConsumer : IEventHandler<SupplierCreated>
{
    public Task Handle(SupplierCreated notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}