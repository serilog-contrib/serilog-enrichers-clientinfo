using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Serilog.Enrichers.ClientInfo.Tests")]

namespace Serilog.Enrichers;

/// <inheritdoc/>
public class ClientIpEnricher : ILogEventEnricher
{
    private const string IpAddressPropertyName = "ClientIp";

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

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        LogEventProperty ipAddressProperty = new(IpAddressPropertyName, new ScalarValue(ipAddress));
        logEvent.AddPropertyIfAbsent(ipAddressProperty);
    }
}