using BuildingBlocks.Domain.Events.External;

namespace Catalog;

public record TestIntegrationEvent(string Data) : IntegrationEvent;