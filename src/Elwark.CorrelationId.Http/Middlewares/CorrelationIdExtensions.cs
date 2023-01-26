using Elwark.CorrelationId.Http.Middlewares;

// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore.Builder;

public static class CorrelationIdExtensions
{
    /// <summary>
    ///     Enables correlation IDs for the request.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
