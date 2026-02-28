using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Common;

namespace WholesaleHub.Api.Infrastructure;

// Maps exceptions to ProblemDetails responses so the frontend always gets the same error shape.
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ve => new ValidationProblemDetails(
                ve.Errors
                    .GroupBy(e => CamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()))
            {
                Status = 400,
                Title = "Some fields are invalid.",
            },
            NotFoundException => new ProblemDetails { Status = 404, Title = "Not found", Detail = exception.Message },
            ConflictException => new ProblemDetails { Status = 409, Title = "Conflict", Detail = exception.Message },
            DomainException => new ProblemDetails { Status = 422, Title = "Can't do that", Detail = exception.Message },
            // don't leak stack traces / sql errors to the client
            _ => new ProblemDetails { Status = 500, Title = "Unexpected error", Detail = "Something went wrong on our side." },
        };

        if (problem.Status >= 500)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", http.Request.Method, http.Request.Path);
        else
            logger.LogInformation("Request rejected ({Status}): {Message}", problem.Status, exception.Message);

        http.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    // FluentValidation gives "UnitPrice", angular forms use "unitPrice"
    private static string CamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
