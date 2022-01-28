using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Exception;
using BuildingBlocks.Validation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Catalogs.Api.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
    {
        AddCustomProblemDetails(builder.Services);

        return builder;
    }

    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(x =>
        {
            // Control when an exception is included
            x.IncludeExceptionDetails = (ctx, _) =>
            {
                // Fetch services from HttpContext.RequestServices
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
                return env.IsDevelopment() || env.IsStaging();
            };
            x.Map<ConflictException>(ex => new ProblemDetails
            {
                Title = "Application rule broken",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
                Type = "https://somedomain/application-rule-validation-error"
            });

            // Exception will produce and returns from our FluentValidation RequestValidationBehavior
            x.Map<ValidationException>(ex => new ProblemDetails
            {
                Title = "input validation rules broken",
                Status = StatusCodes.Status400BadRequest,
                Detail = JsonConvert.SerializeObject(ex.ValidationResultModel.Errors),
                Type = "https://somedomain/input-validation-rules-error"
            });
            x.Map<BadRequestException>(ex => new ProblemDetails
            {
                Title = "bad request exception",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Type = "https://somedomain/bad-request-error"
            });
            x.Map<NotFoundException>(ex => new ProblemDetails
            {
                Title = "not found exception",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message,
                Type = "https://somedomain/not-found-error"
            });
            x.Map<ApiException>(ex => new ProblemDetails
            {
                Title = "api server exception",
                Status = StatusCodes.Status500InternalServerError,
                Detail = ex.Message,
                Type = "https://somedomain/api-server-error"
            });
            x.Map<AppException>(ex => new ProblemDetails
            {
                Title = "application exception",
                Status = StatusCodes.Status500InternalServerError,
                Detail = ex.Message,
                Type = "https://somedomain/application-error"
            });
            x.Map<DomainException>(ex => new ProblemDetails
            {
                Title = "domain exception",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Type = "https://somedomain/domain-error"
            });
            x.Map<IdentityException>(ex =>
            {
                var pd = new ProblemDetails
                {
                    Status = (int)ex.StatusCode,
                    Title = "identity exception",
                    Detail = ex.Message,
                    Type = "https://somedomain/identity-error"
                };

                return pd;
            });

            // Handling Guards exceptions
            x.MapToStatusCode<ArgumentNullException>(StatusCodes.Status400BadRequest);
        });
        return services;
    }
}