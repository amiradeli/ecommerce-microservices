using BuildingBlocks.CQRS.Command;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductBrand;

internal record ChangeProductBrand : ICommand<ChangeProductBrandResult>;

internal record ChangeProductBrandResult;

internal class ChangeProductBrandHandler :
    ICommandHandler<ChangeProductBrand, ChangeProductBrandResult>
{
    public Task<ChangeProductBrandResult> Handle(
        ChangeProductBrand command,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}