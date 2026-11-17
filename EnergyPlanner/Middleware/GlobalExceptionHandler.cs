using Microsoft.AspNetCore.Diagnostics;

namespace EnergyPlanner.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public const string ServerErrorMessage = "Внутрішня помилка сервера. Спробуйте пізніше.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new { message = ServerErrorMessage }, cancellationToken);

        return true;
    }
}
