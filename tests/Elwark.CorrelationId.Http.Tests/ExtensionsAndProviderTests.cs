using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Http.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Elwark.CorrelationId.Http.Tests;

public sealed class ExtensionsAndProviderTests
{
    [Fact]
    public void AddHttpCorrelationIdForwarding_Throws_WhenBuilderIsNull()
    {
        IHttpClientBuilder? builder = null;

        Assert.Throws<ArgumentNullException>(() => builder!.AddHttpCorrelationIdForwarding());
    }

    [Fact]
    public void AddHttpCorrelationIdForwarding_RegistersCorrelationIdHandler()
    {
        var services = new ServiceCollection();

        services.AddHttpClient("test")
            .AddHttpCorrelationIdForwarding();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(CorrelationIdHandler)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void UseCorrelationId_Throws_WhenAppIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => CorrelationIdExtensions.UseCorrelationId(null!));
    }

    [Fact]
    public void UseCorrelationId_ReturnsBuilder()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var app = new ApplicationBuilder(services);

        var result = app.UseCorrelationId();

        Assert.Same(app, result);
    }

    [Fact]
    public void WithTraceIdentifierProvider_RegistersHttpContextAccessorAndProvider()
    {
        var services = new ServiceCollection();

        services.AddCorrelationId()
            .WithTraceIdentifierProvider();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IHttpContextAccessor>());
        Assert.IsType<TraceIdentifierCorrelationIdProvider>(provider.GetRequiredService<ICorrelationIdProvider>());
    }

    [Fact]
    public void TraceIdentifierProvider_ReturnsTraceIdentifier()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-123"
        };
        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };
        var provider = new TraceIdentifierCorrelationIdProvider(accessor);

        var id = provider.GenerateCorrelationId();

        Assert.Equal("trace-123", id);
    }
}
