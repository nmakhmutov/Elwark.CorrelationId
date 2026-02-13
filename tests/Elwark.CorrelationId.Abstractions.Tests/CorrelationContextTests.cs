namespace Elwark.CorrelationId.Abstractions.Tests;

public sealed class CorrelationContextTests
{
    [Fact]
    public void Constructor_SetsValues_WhenInputIsValid()
    {
        var context = new CorrelationContext("cid-1", "X-Correlation-Id");

        Assert.Equal("cid-1", context.CorrelationId);
        Assert.Equal("X-Correlation-Id", context.Header);
    }

    [Fact]
    public void Constructor_UsesDefaultCorrelationId_WhenCorrelationIdIsNull()
    {
        var context = new CorrelationContext(null, "X-Correlation-Id");

        Assert.Equal(CorrelationContext.DefaultCorrelationId, context.CorrelationId);
    }

    [Fact]
    public void Constructor_Throws_WhenHeaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CorrelationContext("cid-1", null!));
    }

    [Fact]
    public void Constructor_Throws_WhenHeaderIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new CorrelationContext("cid-1", string.Empty));
    }
}
