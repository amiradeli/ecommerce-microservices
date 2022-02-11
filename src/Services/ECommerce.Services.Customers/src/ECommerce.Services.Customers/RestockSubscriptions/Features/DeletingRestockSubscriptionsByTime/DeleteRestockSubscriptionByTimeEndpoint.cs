using Ardalis.GuardClauses;
using BuildingBlocks.CQRS.Command;
using BuildingBlocks.Web.MinimalApi;
using ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;

public class DeleteRestockSubscriptionByTimeEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}", DeleteRestockSubscriptionByTime)
            .AllowAnonymous()
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscriptionByTime")
            .WithDisplayName("Delete RestockSubscriptions by time range.");

        return builder;
    }

    private static async Task<IResult> DeleteRestockSubscriptionByTime(
        [FromBody]DeleteRestockSubscriptionByTimeRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new DeleteRestockSubscriptionsByTime(request.From, request.To);

        await commandProcessor.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }
}