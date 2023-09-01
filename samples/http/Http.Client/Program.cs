using Elwark.CorrelationId;
using Elwark.CorrelationId.Abstractions;
using Http.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCorrelationId(options =>
    {
        options.UpdateTraceIdentifier = true;
        options.AddToLoggingScope = true;
    });

builder.Services
    .AddHttpClient<TestClient>(client => client.BaseAddress = new Uri("http://localhost:5140"))
    .AddHttpCorrelationIdForwarding();

var app = builder.Build();

await using (var provider = app.Services.CreateAsyncScope())
{
    using var factory = provider.ServiceProvider.GetRequiredService<ICorrelationContextFactory>();
    factory.Create("MY-CUSTOM-CORRELATION-Id-VALUE", CorrelationIdOptions.DefaultHeader);

    var client = provider.ServiceProvider.GetRequiredService<TestClient>();
    var result = await client.GetAsync();

    Console.WriteLine(result);
}

app.Run();

namespace Http.Client
{
    internal sealed class TestClient
    {
        private readonly HttpClient _client;

        public TestClient(HttpClient client) =>
            _client = client;

        public Task<string> GetAsync() =>
            _client.GetStringAsync("/");
    }
}
