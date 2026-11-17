using Microsoft.AspNetCore.Diagnostics;

namespace EnergyPlanner.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public const string ServerErrorMessage = "Внутрішня помилка сервера. Спробуйте пізніше.";
    public const string InvalidInputMessage = "Некоректні вхідні дані.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var message = ServerErrorMessage;

        if (exception is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = InvalidInputMessage;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);

        return true;
    }
}
