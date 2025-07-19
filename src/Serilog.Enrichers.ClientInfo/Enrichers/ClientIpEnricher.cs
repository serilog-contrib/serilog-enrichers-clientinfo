using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

[assembly: InternalsVisibleTo("Serilog.Enrichers.ClientInfo.Tests")]

namespace Serilog.Enrichers;

/// <inheritdoc />
public class ClientIpEnricher : ILogEventEnricher
{
    private const string IpAddressPropertyName = "ClientIp";
    private const string IpAddressItemKey = "Serilog_ClientIp";

    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientIpEnricher" /> class.
    /// </summary>
    public ClientIpEnricher() : this(new HttpContextAccessor())
    {
    }

    internal ClientIpEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (httpContext.Items.TryGetValue(IpAddressItemKey, out object value) &&
            value is LogEventProperty logEventProperty)
        {
            if (!((ScalarValue)logEventProperty.Value).Value.ToString()!.Equals(ipAddress))
                logEventProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));

            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        LogEventProperty ipAddressProperty = new(IpAddressPropertyName, new ScalarValue(ipAddress));
        httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);
        logEvent.AddPropertyIfAbsent(ipAddressProperty);
    }
}