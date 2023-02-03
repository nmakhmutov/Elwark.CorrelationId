using Elwark.CorrelationId;
using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Grpc.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable CheckNamespace
namespace Grpc.Net.ClientFactory;

public static class GrpcClientFactoryOptionsExtensions
{
    public static void AddCorrelationIdForwarding(this GrpcClientFactoryOptions options)
    {
        var interceptor = new InterceptorRegistration(InterceptorScope.Channel, provider =>
            new CorrelationIdClientInterceptor(
                provider.GetRequiredService<ICorrelationContextAccessor>(),
                provider.GetRequiredService<IOptions<CorrelationIdOptions>>()
            )
        );

        options.InterceptorRegistrations.Add(interceptor);
    }
}
