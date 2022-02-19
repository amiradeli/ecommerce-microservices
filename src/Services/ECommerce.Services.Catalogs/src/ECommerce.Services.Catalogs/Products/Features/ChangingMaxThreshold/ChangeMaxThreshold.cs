using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.CQRS.Command;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingMaxThreshold;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
