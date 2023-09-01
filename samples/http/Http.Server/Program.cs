using Elwark.CorrelationId.Abstractions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddCorrelationId(options => options.UpdateTraceIdentifier = true)
    .WithTraceIdentifierProvider();

builder.Host
    .UseSerilog((_, configuration) =>
        configuration
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{CorrelationId}] {Message:lj}{NewLine}{Exception}"
            )
    );

var app = builder.Build();

app.UseCorrelationId();

app.MapGet("/", (ICorrelationContextAccessor accessor) => Results.Ok(accessor.CorrelationContext));

app.Run();
