using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCorrelationId_Throws_WhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddCorrelationId(null!));
    }

    [Fact]
    public void AddCorrelationIdWithConfigure_Throws_WhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCorrelationId(null!));
    }

    [Fact]
    public void AddCorrelationId_RegistersRequiredServices()
    {
        var services = new ServiceCollection();

        services.AddCorrelationId();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<CorrelationContextAccessor>(provider.GetRequiredService<ICorrelationContextAccessor>());
        Assert.IsType<GuidCorrelationIdProvider>(provider.GetRequiredService<ICorrelationIdProvider>());
        Assert.IsType<CorrelationContextFactory>(provider.GetRequiredService<ICorrelationContextFactory>());
    }

    [Fact]
    public void AddCorrelationIdWithConfigure_ThrowsOptionsValidationException_WhenRequestHeaderIsEmpty()
    {
        var services = new ServiceCollection();
        services.AddCorrelationId(options => options.RequestHeader = string.Empty);
        using var provider = services.BuildServiceProvider();

        var accessor = provider.GetRequiredService<IOptions<CorrelationIdOptions>>();

        Assert.Throws<OptionsValidationException>(() => _ = accessor.Value);
    }

    [Fact]
    public void AddCorrelationIdWithConfigure_ThrowsOptionsValidationException_WhenLoggingScopeKeyIsEmpty()
    {
        var services = new ServiceCollection();
        services.AddCorrelationId(options =>
        {
            options.AddToLoggingScope = true;
            options.LoggingScopeKey = string.Empty;
        });
        using var provider = services.BuildServiceProvider();

        var accessor = provider.GetRequiredService<IOptions<CorrelationIdOptions>>();

        Assert.Throws<OptionsValidationException>(() => _ = accessor.Value);
    }

    [Fact]
    public void AddCorrelationIdWithConfigure_ThrowsOptionsValidationException_WhenResponseHeaderIsEmpty()
    {
        var services = new ServiceCollection();
        services.AddCorrelationId(options =>
        {
            options.IncludeInResponse = true;
            options.ResponseHeader = string.Empty;
        });
        using var provider = services.BuildServiceProvider();

        var accessor = provider.GetRequiredService<IOptions<CorrelationIdOptions>>();

        Assert.Throws<OptionsValidationException>(() => _ = accessor.Value);
    }

    [Fact]
    public void WithCustomProvider_RegistersCustomProvider()
    {
        var services = new ServiceCollection();

        services.AddCorrelationId()
            .WithCustomProvider<TestCorrelationIdProvider>();

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ICorrelationIdProvider>();

        Assert.IsType<TestCorrelationIdProvider>(resolved);
    }

    [Fact]
    public void WithCustomProvider_Throws_WhenBuilderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.WithCustomProvider<TestCorrelationIdProvider>(null!));
    }

    private sealed class TestCorrelationIdProvider : ICorrelationIdProvider
    {
        public string GenerateCorrelationId() => "test-correlation-id";
    }
}
