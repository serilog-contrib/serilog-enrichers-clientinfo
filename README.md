# serilog-enrichers-clientinfo [![NuGet](http://img.shields.io/nuget/v/Serilog.Enrichers.ClientInfo.svg?style=flat)](https://www.nuget.org/packages/Serilog.Enrichers.ClientInfo/)
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

For `ClientIp` enricher you can configure the `x-forwarded-for` header if the proxy server uses a different header to forward the IP address.
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp(headerName: "CF-Connecting-IP")
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
        "Name": "WithClientIp",
        "Args": {
          "headerName": "CF-Connecting-IP"
        }
      }
    ],
  }
}
```

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

You can use multiple `WithRequestHeader` to log different request headers.
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithRequestHeader(headerName: "header-name-1")
    .Enrich.WithRequestHeader(headerName: "header-name-2")
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
To include logged headers in `OutputTemplate`, the header name without `-` should be used. For example, if the header name is `User-Agent`, you should use `UserAgent`.
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
                .Enrich.WithRequestHeader("another-header-name", "some-property-name")
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
