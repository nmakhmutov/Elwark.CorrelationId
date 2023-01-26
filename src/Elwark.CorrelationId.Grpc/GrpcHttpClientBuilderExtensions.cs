using Elwark.CorrelationId.Grpc.Interceptors;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class GrpcHttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddGrpcCorrelationIdForwarding(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<CorrelationIdClientInterceptor>();
        builder.AddInterceptor<CorrelationIdClientInterceptor>();

        return builder;
    }
}
