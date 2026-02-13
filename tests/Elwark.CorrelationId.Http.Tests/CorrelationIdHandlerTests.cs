using System.Net;
using Elwark.CorrelationId.Abstractions;

namespace Elwark.CorrelationId.Http.Tests;

public sealed class CorrelationIdHandlerTests
{
    [Fact]
    public async Task SendAsync_DoesNotAddHeader_WhenContextIsMissing()
    {
        var accessor = new TestCorrelationContextAccessor();
        var inner = new CapturingHandler();
        var handler = new CorrelationIdHandler(accessor)
        {
            InnerHandler = inner
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        Assert.NotNull(inner.LastRequest);
        Assert.Empty(inner.LastRequest!.Headers);
    }

    [Fact]
    public async Task SendAsync_AddsHeader_WhenContextExistsAndHeaderIsMissing()
    {
        var accessor = new TestCorrelationContextAccessor
        {
            CorrelationContext = new CorrelationContext("cid-1", "X-Correlation-Id")
        };
        var inner = new CapturingHandler();
        var handler = new CorrelationIdHandler(accessor)
        {
            InnerHandler = inner
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal("cid-1", inner.LastRequest!.Headers.GetValues("X-Correlation-Id").Single());
    }

    [Fact]
    public async Task SendAsync_DoesNotDuplicateHeader_WhenHeaderAlreadyExists()
    {
        var accessor = new TestCorrelationContextAccessor
        {
            CorrelationContext = new CorrelationContext("cid-1", "X-Correlation-Id")
        };
        var inner = new CapturingHandler();
        var handler = new CorrelationIdHandler(accessor)
        {
            InnerHandler = inner
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Add("X-Correlation-Id", "existing-id");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        var values = inner.LastRequest!.Headers.GetValues("X-Correlation-Id").ToArray();
        Assert.Single(values);
        Assert.Equal("existing-id", values[0]);
    }

    private sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public CorrelationContext? CorrelationContext { get; set; }
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
