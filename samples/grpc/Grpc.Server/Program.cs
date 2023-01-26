using Grpc.AspNetCore.Server;
using Grpc.Server.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCorrelationId();

builder.Services
    .AddGrpc(options => options.UseCorrelationId());

builder.Host
    .UseSerilog((_, configuration) =>
        configuration
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    );

var app = builder.Build();

app.MapGrpcService<GreeterService>();

app.Run();
