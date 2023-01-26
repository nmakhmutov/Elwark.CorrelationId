using Elwark.CorrelationId;
using Elwark.CorrelationId.Abstractions;
using Grpc;
using Grpc.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCorrelationId(options => options.AddToLoggingScope = true);

builder.Services
    .AddGrpc();

builder.Services
    .AddGrpcClient<Greeter.GreeterClient>(options => options.Address = new Uri("http://localhost:5251"))
    .AddGrpcCorrelationIdForwarding();

builder.Host
    .UseSerilog((_, configuration) =>
        configuration
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    );

var app = builder.Build();

await using var provider = app.Services.CreateAsyncScope();
using var factory = provider.ServiceProvider.GetRequiredService<ICorrelationContextFactory>();
factory.Create("MY-CUSTOM-CORRELATION-Id-VALUE", CorrelationIdOptions.DefaultHeader);

var client = provider.ServiceProvider.GetRequiredService<Greeter.GreeterClient>();
await client.UnaryAsync(new HelloRequest { Name = "Unary test" });

var clientStreaming = client.ClientStreaming();
await clientStreaming.RequestStream.WriteAsync(new HelloRequest { Name = "Client streaming test" });
await clientStreaming.RequestStream.CompleteAsync();

var serverStreaming = client.ServerStreaming(new HelloRequest { Name = "Server streaming test" });
await foreach (var _ in serverStreaming.ResponseStream.ReadAllAsync())
{
}

var duplexStreaming = client.DuplexStreaming();
await duplexStreaming.RequestStream.WriteAsync(new HelloRequest { Name = "Duplex streaming test" });
await duplexStreaming.RequestStream.CompleteAsync();
await foreach (var _ in duplexStreaming.ResponseStream.ReadAllAsync())
{
}
