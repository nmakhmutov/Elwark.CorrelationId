using Elwark.CorrelationId.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    ///     Adds a handler which forwards the correlation ID by attaching it to the headers on outgoing requests.
    /// </summary>
    /// <remarks>
    ///     The header name will match the name of the incoming request header.
    /// </remarks>
    /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHttpClientBuilder AddHttpCorrelationIdForwarding(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Services.TryAddTransient<CorrelationIdHandler>();
        builder.AddHttpMessageHandler<CorrelationIdHandler>();

        return builder;
    }
}
