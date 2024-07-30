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
    .WriteTo.Sink(new DelegatingSink(e => LogEvent = e))
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

app
    .UseForwardedHeaders()
    .Use(
        async (context, next) =>
        {
            app.Logger.LogInformation(
                "HttpContext.Connection.RemoteIpAddress: {RemoteIpAddress}, HttpContext.Items[\"Serilog_ClientIp\"]: {Serilog_ClientIp}",
                context.Connection.RemoteIpAddress,
                (context.Items["Serilog_ClientIp"] as LogEventProperty)?.Value);

            await next(context);
        });

app.MapGet("/", () => "hello world");

app.Run();

public partial class Program
{
    private Program()
    { }

    public static LogEvent LogEvent;
}