# Elwark Correlation Id

A Correlation Id is a tool used in distributed systems to trace requests across multiple services. This specific library 
offers a lightweight approach to implementing Correlation Ids. When enabled, it checks for the presence of a Correlation
Id in request headers and attaches it to the Correlation Context, which can then be used for logging and other purposes. 
Additionally, this Correlation Id can also be attached to downstream HTTP calls made through an `IHttpClientFactory`. 
However, it should be noted that this library may not be fully comprehensive and that built-in tracing options for .NET apps
may be more suitable for more extensive tracing needs.

## Installation

You should install [Elwark.CorrelationId](https://www.nuget.org/packages/Elwark.CorrelationId) from NuGet:

```ps
Install-Package Elwark.CorrelationId
```

[![NuGet](https://img.shields.io/nuget/v/Elwark.CorrelationId.svg)](https://www.nuget.org/packages/Elwark.CorrelationId)

This command from Package Manager Console will download and install CorrelationId and all required dependencies.

All stable and some pre-release packages are available on NuGet. 

## Quick Start

### Register with DI

Inside `ConfigureServices` add the required correlation id services, with common defaults.

```csharp
services.AddCorrelationId()
```

or you can define with options

```csharp
services.AddCorrelationId(options =>
{ 
    options.AddToLoggingScope = true;
    options.EnforceHeader = true;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.RequestHeader = "X-Correlation-Id";
    options.ResponseHeader = "X-Correlation-Id";
    options.UpdateTraceIdentifier = false;
});
```

This registers a correlation id provider which generates new ids based on a random GUID.

## Minimal API / MVC

You should install [Elwark.CorrelationId.Http](https://www.nuget.org/packages/Elwark.CorrelationId.Http) from NuGet:

```ps
Install-Package Elwark.CorrelationId.Http
```

[![NuGet](https://img.shields.io/nuget/v/Elwark.CorrelationId.Http.svg)](https://www.nuget.org/packages/Elwark.CorrelationId.Http)

### Add the middleware

Register the middleware into the pipeline. This should occur before any downstream middleware which requires the correlation ID. Normally this will be registered very early in the middleware pipeline.

```csharp
app.UseCorrelationId();
```

Also Elwark.CorrelationId.Http contains TraceIdentifier Correlation id provider. You can replace GUID provider by TraceId just following code:

```csharp
services.AddCorrelationId()
   .WithTraceIdentifierProvider();
```

After your correlation id will be trace identifier from HttpContext.

### Add HttpClient forwarding

Forwarding your correlation id to another service by http call available by adding:

```csharp
builder.Services
    .AddHttpClient<ITestClient, TestClient>()
    .AddHttpCorrelationIdForwarding();
```

## gRPC
You should install [Elwark.CorrelationId.Grpc](https://www.nuget.org/packages/Elwark.CorrelationId.Grpc) from NuGet:

```ps
Install-Package Elwark.CorrelationId.Grpc
```

[![NuGet](https://img.shields.io/nuget/v/Elwark.CorrelationId.Grpc.svg)](https://www.nuget.org/packages/Elwark.CorrelationId.Grpc)

### Add the interceptor

Register the interceptor into the pipeline.

```csharp
builder.Services
    .AddGrpc(options => options.UseCorrelationId());
```

### Add GrpcClient forwarding

Forwarding your correlation id to another service by gRPC call available by adding:

```csharp
builder.Services
    .AddGrpcClient<Greeter.GreeterClient>()
    .AddGrpcCorrelationIdForwarding();
```

or

```csharp
builder.Services
    .AddGrpcClient<Greeter.GreeterClient>(options =>
    {
        options.AddCorrelationIdForwarding();
    });
```
Where you need to access the correlation Id, you may request the `ICorrelationContextAccessor` from DI.

```csharp
public class MyClass
{
   private readonly ICorrelationContextAccessor _accessor;

   public MyClass(ICorrelationContextAccessor accessor)
   {
	  _accessor = accessor;
   }

   ...
}
```
