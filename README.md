# serilog-enrichers-clientinfo [![NuGet](http://img.shields.io/nuget/v/Serilog.Enrichers.ClientInfo.svg?style=flat)](https://www.nuget.org/packages/Serilog.Enrichers.ClientInfo/)
Enrich logs with client IP and UserAgent.

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
    .Enrich.WithClientAgent()
    // ...other configuration...
    .CreateLogger();
```

or in `appsttings.json` file:
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Using":  [ "Serilog.Enrichers" ],
    "Enrich": [ "WithClientIp", "WithClientAgent"],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

The `WithClientIp()` enricher will add a `ClientIp` property and the `WithClientAgent()` enricher will add a `ClientAgent` property to produced events.

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
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {ClientIp} {ClientAgent}] {Message:lj}{NewLine}{Exception}")
                .Enrich.WithClientIp()
                .Enrich.WithClientAgent()
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
