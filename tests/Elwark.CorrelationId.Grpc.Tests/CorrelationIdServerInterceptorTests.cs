using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Grpc.Interceptors;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Grpc.Tests;

public sealed class CorrelationIdServerInterceptorTests
{
    [Fact]
    public async Task UnaryServerHandler_ThrowsInvalidArgument_WhenHeaderIsRequiredAndMissing()
    {
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var interceptor = CreateInterceptor(
            provider,
            factory,
            new CorrelationIdOptions
            {
                EnforceHeader = true,
                RequestHeader = "x-request-id"
            }
        );
        var context = new TestServerCallContext(new Metadata());

        var exception = await Assert.ThrowsAsync<RpcException>(() =>
            interceptor.UnaryServerHandler(
                "request",
                context,
                (_, _) => Task.FromResult("response")
            ));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Contains("x-request-id", exception.Status.Detail);
    }

    [Fact]
    public async Task UnaryServerHandler_WritesResponseHeader_UsingIncomingRequestHeader()
    {
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var interceptor = CreateInterceptor(
            provider,
            factory,
            new CorrelationIdOptions
            {
                RequestHeader = "x-request-id",
                ResponseHeader = "x-response-id"
            }
        );
        var context = new TestServerCallContext(new Metadata
        {
            new("x-request-id", "incoming-id")
        });

        var response = await interceptor.UnaryServerHandler(
            "request",
            context,
            (_, _) => Task.FromResult("response")
        );

        Assert.Equal("response", response);
        Assert.NotNull(context.WrittenResponseHeaders);
        Assert.Contains(
            context.WrittenResponseHeaders!,
            entry => entry.Key == "x-response-id" && entry.Value == "incoming-id");
        Assert.Equal("incoming-id", factory.LastCreated!.CorrelationId);
        Assert.True(factory.Disposed);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task UnaryServerHandler_UsesProviderValue_WhenIgnoreRequestHeaderIsEnabled()
    {
        var provider = new FixedCorrelationIdProvider("generated-id");
        var factory = new RecordingCorrelationContextFactory();
        var interceptor = CreateInterceptor(
            provider,
            factory,
            new CorrelationIdOptions
            {
                IgnoreRequestHeader = true,
                RequestHeader = "x-request-id",
                ResponseHeader = "x-response-id"
            }
        );
        var context = new TestServerCallContext(new Metadata
        {
            new("x-request-id", "incoming-id")
        });

        _ = await interceptor.UnaryServerHandler(
            "request",
            context,
            (_, _) => Task.FromResult("response")
        );

        Assert.NotNull(context.WrittenResponseHeaders);
        Assert.Contains(
            context.WrittenResponseHeaders!,
            entry => entry.Key == "x-response-id" && entry.Value == "generated-id");
        Assert.Equal("generated-id", factory.LastCreated!.CorrelationId);
        Assert.Equal(1, provider.CallCount);
    }

    private static CorrelationIdServerInterceptor CreateInterceptor(
        ICorrelationIdProvider provider,
        ICorrelationContextFactory factory,
        CorrelationIdOptions options) =>
        new(
            provider,
            NullLogger<CorrelationIdServerInterceptor>.Instance,
            Options.Create(options),
            factory
        );

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

    private sealed class TestServerCallContext(Metadata requestHeaders) : ServerCallContext
    {
        private readonly Dictionary<object, object> _userState = [];

        public Metadata? WrittenResponseHeaders { get; private set; }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            WrittenResponseHeaders = responseHeaders;
            return Task.CompletedTask;
        }

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
            throw new NotSupportedException();

        protected override string MethodCore => "test.Service/Unary";

        protected override string HostCore => "localhost";

        protected override string PeerCore => "127.0.0.1";

        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);

        protected override Metadata RequestHeadersCore => requestHeaders;

        protected override CancellationToken CancellationTokenCore => CancellationToken.None;

        protected override Metadata ResponseTrailersCore { get; } = [];

        protected override Status StatusCore { get; set; } = Status.DefaultSuccess;

        protected override WriteOptions? WriteOptionsCore { get; set; }

        protected override AuthContext AuthContextCore =>
            new(string.Empty, new Dictionary<string, List<AuthProperty>>());

        protected override IDictionary<object, object> UserStateCore => _userState;
    }
}
