using Ardalis.GuardClauses;
using BuildingBlocks.CQRS.Command;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer;

public static class CreateCustomerEndpoint
{
    internal static IEndpointRouteBuilder MapCreateCustomerEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(CustomersConfigs.CustomersPrefixUri, CreateCustomer)
            .AllowAnonymous()
            .WithTags(CustomersConfigs.Tag)
            .Produces<CreateCustomerResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.");

        return endpoints;
    }

    private static async Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new CreateCustomer(request.Email);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Created($"{CustomersConfigs.CustomersPrefixUri}/{result.CustomerId}", result);
    }
}
