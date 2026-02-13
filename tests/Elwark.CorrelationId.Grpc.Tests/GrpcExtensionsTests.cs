using Grpc.AspNetCore.Server;
using Grpc.Net.ClientFactory;

namespace Elwark.CorrelationId.Grpc.Tests;

public sealed class GrpcExtensionsTests
{
    [Fact]
    public void UseCorrelationId_AddsServerInterceptorAndReturnsOptions()
    {
        var options = new GrpcServiceOptions();
        var before = options.Interceptors.Count;

        var result = options.UseCorrelationId();

        Assert.Same(options, result);
        Assert.Equal(before + 1, options.Interceptors.Count);
    }

    [Fact]
    public void AddCorrelationIdForwarding_AddsClientInterceptorRegistration()
    {
        var options = new GrpcClientFactoryOptions();
        var before = options.InterceptorRegistrations.Count;

        options.AddCorrelationIdForwarding();

        Assert.Equal(before + 1, options.InterceptorRegistrations.Count);
    }
}
