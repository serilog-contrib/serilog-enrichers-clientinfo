using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Serilog.Enrichers.ClientInfo.Tests")]

namespace Serilog.Enrichers;

public class ClientIpEnricher : ILogEventEnricher
{
    private const string IpAddressPropertyName = "ClientIp";
    private const string IpAddressItemKey = "Serilog_ClientIp";

    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ClientIpEnricher"/> class.
    /// </summary>
    public ClientIpEnricher() : this(new HttpContextAccessor())
    {
    }

    internal ClientIpEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (httpContext.Items[IpAddressItemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var ipAddress = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ipAddress = "unknown";
        }

        var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));
        httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);

        logEvent.AddPropertyIfAbsent(ipAddressProperty);
    }
}