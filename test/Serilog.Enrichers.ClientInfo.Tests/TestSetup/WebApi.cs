using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.ClientInfo.Tests;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .Enrich.WithRequestHeader("X-Forwarded-For")
    .WriteTo.Sink(new DelegatingSink(e => LogEvent = e, saveLogs: true))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services
    .AddHttpContextAccessor()
    .Configure<ForwardedHeadersOptions>(
        options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

var app = builder.Build();

app.UseForwardedHeaders();

app.MapGet("/", () => "hello world");

app.Run();

public partial class Program
{
    private Program()
    { }

    public static LogEvent LogEvent;
}