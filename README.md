# serilog-enrichers-clientinfo [![NuGet](http://img.shields.io/nuget/v/Serilog.Enrichers.ClientInfo.svg?style=flat)](https://www.nuget.org/packages/Serilog.Enrichers.ClientInfo/) [![](https://img.shields.io/nuget/dt/Serilog.Enrichers.ClientInfo.svg?label=nuget%20downloads)](Serilog.Enrichers.ClientInfo)

Enrich logs with client IP, Correlation Id and HTTP request headers.

Install the _Serilog.Enrichers.ClientInfo_ [NuGet package](https://www.nuget.org/packages/Serilog.Enrichers.ClientInfo/)

```powershell
Install-Package Serilog.Enrichers.ClientInfo
```
or
```shell
dotnet add package Serilog.Enrichers.ClientInfo
```

Apply the enricher to your `LoggerConfiguration` in code:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .Enrich.WithCorrelationId()
    .Enrich.WithRequestHeader("Header-Name1")
    // ...other configuration...
    .CreateLogger();
```

or in `appsettings.json` file:
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Using":  [ "Serilog.Enrichers.ClientInfo" ],
    "Enrich": [
      "WithClientIp",
      "WithCorrelationId",
      {
          "Name": "WithRequestHeader",
          "Args": { "headerName": "User-Agent"}
      }
    ],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

---

### ClientIp
`ClientIp` enricher reads client IP from `HttpContext.Connection.RemoteIpAddress`. Since version 2.1, for [security reasons](https://nvd.nist.gov/vuln/detail/CVE-2023-22474), it no longer reads the `x-forwarded-for` header. To handle forwarded headers, configure [ForwardedHeadersOptions](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-7.0#forwarded-headers-middleware-order). If you still want to log `x-forwarded-for`, you can use the `RequestHeader` enricher.

#### Basic Usage
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    ...
```
or
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Using":  [ "Serilog.Enrichers.ClientInfo" ],
    "Enrich": [
      {
        "Name": "WithClientIp"
      }
    ],
  }
}
```

#### IP Version Preferences
You can configure the enricher to prefer or filter specific IP versions (IPv4 or IPv6):

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp(IpVersionPreference.Ipv4Only)
    ...
```

Available IP version preferences:
- `None` (default): No preference - use whatever IP version is available
- `PreferIpv4`: Prefer IPv4 addresses when multiple are available, fallback to IPv6
- `PreferIpv6`: Prefer IPv6 addresses when multiple are available, fallback to IPv4  
- `Ipv4Only`: Only log IPv4 addresses, ignore IPv6 addresses
- `Ipv6Only`: Only log IPv6 addresses, ignore IPv4 addresses
  
### CorrelationId
For `CorrelationId` enricher you can:
- Configure the header name and default header name is `x-correlation-id`
- Set value for correlation id when the header is not available in request header collection and the default value is false
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithCorrelationId(headerName: "correlation-id", addValueIfHeaderAbsence: true)
    ...
```
or
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Using":  [ "Serilog.Enrichers.ClientInfo" ],
    "Enrich": [
      {
        "Name": "WithCorrelationId",
        "Args": {
          "headerName": "correlation-id"
          "addValueIfHeaderAbsence": true
        }
      }
    ],
  }
}
```
#### Retrieving Correlation ID
You can easily retrieve the correlation ID from `HttpContext` using the `GetCorrelationId()` extension method:
```csharp
public void SomeControllerAction()
{
    // This will return the correlation ID that was enriched by the CorrelationIdEnricher
    var correlationId = HttpContext.GetCorrelationId();

    if (!string.IsNullOrEmpty(correlationId))
    {
        // You can use it for additional logging/tracing, etc
    }
}
```
This eliminates the need for manual casting and provides a clean API for accessing correlation IDs.

### RequestHeader
You can use multiple `WithRequestHeader` to log different request headers. `WithRequestHeader` accepts two parameters; The first parameter `headerName` is the header name to log 
and the second parameter is `propertyName` which is the log property name.
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithRequestHeader(headerName: "header-name-1")
    .Enrich.WithRequestHeader(headerName: "header-name-2", propertyName: "SomeHeaderName")
    ...
```
or
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Using":  [ "Serilog.Enrichers.ClientInfo" ],
    "Enrich": [
      {
        "Name": "WithRequestHeader",
        "Args": {
          "headerName": "User-Agent"
        }
      },
      {
        "Name": "WithRequestHeader",
        "Args": {
          "headerName": "Connection"
        }
      },
      {
        "Name": "WithRequestHeader",
        "Args": {
          "headerName": "Content-Length",
          "propertyName": "RequestLength"
        }
      }
    ],
  }
}
```

#### Note
To include logged headers in `OutputTemplate`, the header name without `-` should be used if you haven't set the log property name. For example, if the header name is `User-Agent`, you should use `UserAgent`.
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithRequestHeader("User-Agent")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} {UserAgent} {Message:lj}{NewLine}{Exception}")
```

## Installing into an ASP.NET Core Web Application
You need to register the `IHttpContextAccessor` singleton so the enrichers have access to the requests `HttpContext` to extract client IP and client agent.
This is what your `Startup` class should contain in order for this enricher to work as expected:

```cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MyWebApp
{
    public class Startup
    {
        public Startup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} CLient IP: {ClientIp} Correlation Id: {CorrelationId} header-name: {headername} {Message:lj}{NewLine}{Exception}")
                .Enrich.WithClientIp()
                .Enrich.WithCorrelationId()
                .Enrich.WithRequestHeader("header-name")
                .Enrich.WithRequestHeader("another-header-name", "SomePropertyName")
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // ...
            services.AddHttpContextAccessor();
            // ...
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // ...
            loggerFactory.AddSerilog();
            // ...
        }
    }
}
```
