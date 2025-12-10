using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc />
public class ClientIpEnricher : ILogEventEnricher
{
    private const string IpAddressPropertyName = "ClientIp";
    private const string IpAddressItemKey = "Serilog_ClientIp";

    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IpVersionPreference _ipVersionPreference;
    private readonly string _ipAddressPropertyName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientIpEnricher" /> class.
    /// </summary>
    public ClientIpEnricher() : this(new HttpContextAccessor())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientIpEnricher" /> class.
    /// </summary>
    /// <param name="ipVersionPreference">The IP version preference for filtering IP addresses.</param>
    public ClientIpEnricher(IpVersionPreference ipVersionPreference) : this(new HttpContextAccessor(),
        ipVersionPreference)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientIpEnricher" /> class.
    /// </summary>
    /// <param name="ipVersionPreference">The IP version preference for filtering IP addresses.</param>
    /// <param name="ipAddressPropertyName">The custom property name for the IP address log property.</param>
    public ClientIpEnricher(IpVersionPreference ipVersionPreference, string ipAddressPropertyName)
    {
        ArgumentNullException.ThrowIfNull(ipAddressPropertyName, nameof(ipAddressPropertyName));
        _contextAccessor = new HttpContextAccessor();
        _ipVersionPreference = ipVersionPreference;
        _ipAddressPropertyName = ipAddressPropertyName;
    }

    internal ClientIpEnricher(IHttpContextAccessor contextAccessor,
        IpVersionPreference ipVersionPreference = IpVersionPreference.None,
        string ipAddressPropertyName = IpAddressPropertyName)
    {
        _contextAccessor = contextAccessor;
        _ipVersionPreference = ipVersionPreference;
        _ipAddressPropertyName = ipAddressPropertyName;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        IPAddress remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null) return;

        // Apply IP version filtering based on preference
        IPAddress filteredIpAddress = ApplyIpVersionFilter(remoteIpAddress);
        if (filteredIpAddress == null) return; // IP address was filtered out based on preference

        string ipAddress = filteredIpAddress.ToString();

        if (httpContext.Items.TryGetValue(IpAddressItemKey, out object value) &&
            value is LogEventProperty logEventProperty)
        {
            if (!((ScalarValue)logEventProperty.Value).Value!.ToString()!.Equals(ipAddress))
                logEventProperty = new LogEventProperty(_ipAddressPropertyName, new ScalarValue(ipAddress));

            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        LogEventProperty ipAddressProperty = new(_ipAddressPropertyName, new ScalarValue(ipAddress));
        httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);
        logEvent.AddPropertyIfAbsent(ipAddressProperty);
    }

    /// <summary>
    ///     Applies IP version filtering based on the configured preference.
    /// </summary>
    /// <param name="ipAddress">The IP address to filter.</param>
    /// <returns>The filtered IP address, or null if it should be excluded.</returns>
    private IPAddress ApplyIpVersionFilter(IPAddress ipAddress)
    {
        return _ipVersionPreference switch
        {
            IpVersionPreference.None => ipAddress,
            IpVersionPreference
                .PreferIpv4 => ipAddress, // For single IP, just return it (preference only matters with multiple IPs)
            IpVersionPreference
                .PreferIpv6 => ipAddress, // For single IP, just return it (preference only matters with multiple IPs)
            IpVersionPreference.Ipv4Only => ipAddress.AddressFamily == AddressFamily.InterNetwork ? ipAddress : null,
            IpVersionPreference.Ipv6Only => ipAddress.AddressFamily == AddressFamily.InterNetworkV6 ? ipAddress : null,
            _ => ipAddress
        };
    }
}