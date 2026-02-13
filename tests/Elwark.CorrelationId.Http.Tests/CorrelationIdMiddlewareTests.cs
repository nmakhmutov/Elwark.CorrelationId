using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Http.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Http.Tests;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Invoke_ReturnsBadRequest_WhenHeaderIsRequiredAndMissing()
    {
        var options = new CorrelationIdOptions
        {
            EnforceHeader = true,
            RequestHeader = "X-Request-Id"
        };
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var nextCalled = false;
        var middleware = CreateMiddleware(options, provider, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var (context, responseFeature) = CreateContext();

        await middleware.Invoke(context, factory);
        await responseFeature.FireOnStartingAsync();

        Assert.False(nextCalled);
        Assert.False(factory.Disposed);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal("The 'X-Request-Id' request header is required, but was not found.", payload);
    }

    [Fact]
    public async Task Invoke_UsesRequestHeaderAndWritesResponseHeader_WhenHeaderExists()
    {
        var options = new CorrelationIdOptions();
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var nextCalled = false;
        var middleware = CreateMiddleware(options, provider, async ctx =>
        {
            nextCalled = true;
            await ctx.Response.WriteAsync("ok");
        });

        var (context, responseFeature) = CreateContext();
        context.Request.Headers[options.RequestHeader] = "incoming-id";

        await middleware.Invoke(context, factory);
        await responseFeature.FireOnStartingAsync();

        Assert.True(nextCalled);
        Assert.True(factory.Disposed);
        Assert.NotNull(factory.LastCreated);
        Assert.Equal("incoming-id", factory.LastCreated!.CorrelationId);
        Assert.Equal(options.RequestHeader, factory.LastCreated.Header);
        Assert.Equal("incoming-id", context.Response.Headers[options.ResponseHeader].ToString());
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task Invoke_IgnoresRequestHeader_WhenOptionIsEnabled()
    {
        var options = new CorrelationIdOptions
        {
            IgnoreRequestHeader = true
        };
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var middleware = CreateMiddleware(options, provider, async ctx => await ctx.Response.WriteAsync("ok"));

        var (context, responseFeature) = CreateContext();
        context.Request.Headers[options.RequestHeader] = "incoming-id";

        await middleware.Invoke(context, factory);
        await responseFeature.FireOnStartingAsync();

        Assert.Equal("generated-id", factory.LastCreated!.CorrelationId);
        Assert.Equal("generated-id", context.Response.Headers[options.ResponseHeader].ToString());
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task Invoke_UpdatesTraceIdentifier_WhenOptionIsEnabled()
    {
        var options = new CorrelationIdOptions
        {
            UpdateTraceIdentifier = true
        };
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var middleware = CreateMiddleware(options, provider, async ctx => await ctx.Response.WriteAsync("ok"));

        var (context, _) = CreateContext();

        await middleware.Invoke(context, factory);

        Assert.Equal("generated-id", context.TraceIdentifier);
    }

    [Fact]
    public async Task Invoke_DoesNotWriteResponseHeader_WhenIncludeInResponseIsDisabled()
    {
        var options = new CorrelationIdOptions
        {
            IncludeInResponse = false
        };
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var middleware = CreateMiddleware(options, provider, async ctx => await ctx.Response.WriteAsync("ok"));

        var (context, responseFeature) = CreateContext();

        await middleware.Invoke(context, factory);
        await responseFeature.FireOnStartingAsync();

        Assert.False(context.Response.Headers.ContainsKey(options.ResponseHeader));
    }

    private static CorrelationIdMiddleware CreateMiddleware(
        CorrelationIdOptions options,
        ICorrelationIdProvider provider,
        RequestDelegate next) =>
        new(
            NullLogger<CorrelationIdMiddleware>.Instance,
            next,
            Options.Create(options),
            provider
        );

    private static (DefaultHttpContext Context, TestHttpResponseFeature ResponseFeature) CreateContext()
    {
        var responseFeature = new TestHttpResponseFeature();
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Response.Body = responseFeature.Body;

        return (context, responseFeature);
    }

    private sealed class FixedCorrelationIdProvider(string value) : ICorrelationIdProvider
    {
        public int CallCount { get; private set; }

        public string GenerateCorrelationId()
        {
            CallCount++;
            return value;
        }
    }

    private sealed class RecordingCorrelationContextFactory : ICorrelationContextFactory
    {
        public CorrelationContext? LastCreated { get; private set; }
        public bool Disposed { get; private set; }

        public CorrelationContext Create(string correlationId, string header)
        {
            LastCreated = new CorrelationContext(correlationId, header);
            return LastCreated;
        }

        public void Dispose() =>
            Disposed = true;
    }

    private sealed class TestHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStarting = [];

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state) =>
            _onStarting.Add((callback, state));

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public async Task FireOnStartingAsync()
        {
            foreach (var (callback, state) in Enumerable.Reverse(_onStarting))
                await callback(state);

            HasStarted = true;
        }
    }
}
