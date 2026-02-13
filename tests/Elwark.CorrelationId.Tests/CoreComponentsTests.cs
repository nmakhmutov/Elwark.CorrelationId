using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Providers;

namespace Elwark.CorrelationId.Tests;

public sealed class CoreComponentsTests
{
    [Fact]
    public void CorrelationContextAccessor_GetSet_Works()
    {
        var accessor = new CorrelationContextAccessor();
        var context = new CorrelationContext("cid-1", "X-Correlation-Id");

        accessor.CorrelationContext = context;

        Assert.Same(context, accessor.CorrelationContext);
    }

    [Fact]
    public void CorrelationContextFactory_Create_SetsAccessorValue()
    {
        var accessor = new CorrelationContextAccessor();
        var factory = new CorrelationContextFactory(accessor);

        var created = factory.Create("cid-1", "X-Correlation-Id");

        Assert.Same(created, accessor.CorrelationContext);
        Assert.Equal("cid-1", created.CorrelationId);
    }

    [Fact]
    public void CorrelationContextFactory_Dispose_ClearsAccessorValue()
    {
        var accessor = new CorrelationContextAccessor();
        var factory = new CorrelationContextFactory(accessor);
        _ = factory.Create("cid-1", "X-Correlation-Id");

        factory.Dispose();

        Assert.Null(accessor.CorrelationContext);
    }

    [Fact]
    public void GuidCorrelationIdProvider_ReturnsGuid()
    {
        var provider = new GuidCorrelationIdProvider();

        var value = provider.GenerateCorrelationId();

        Assert.True(Guid.TryParse(value, out _));
    }
}
