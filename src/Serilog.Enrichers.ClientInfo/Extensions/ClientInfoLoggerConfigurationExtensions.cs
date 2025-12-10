using System;
using Microsoft.AspNetCore.Http;
using Serilog.Configuration;
using Serilog.Enrichers;

namespace Serilog;

/// <summary>
///     Extension methods for setting up client IP, client agent and correlation identifier enrichers
///     <see cref="LoggerEnrichmentConfiguration" />.
/// </summary>
public static class ClientInfoLoggerConfigurationExtensions
{
    /// <summary>
    ///     Registers the client IP enricher to enrich logs with
    ///     <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress" /> value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With<ClientIpEnricher>();
    }

    /// <summary>
    ///     Registers the client IP enricher to enrich logs with
    ///     <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress" /> value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="ipVersionPreference">The IP version preference for filtering IP addresses.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        IpVersionPreference ipVersionPreference)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new ClientIpEnricher(ipVersionPreference));
    }

    /// <summary>
    ///     Registers the client IP enricher to enrich logs with
    ///     <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress" /> value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="ipAddressPropertyName">The custom property name for the IP address log property.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string ipAddressPropertyName)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new ClientIpEnricher(IpVersionPreference.None, ipAddressPropertyName));
    }

    /// <summary>
    ///     Registers the client IP enricher to enrich logs with
    ///     <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress" /> value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="ipVersionPreference">The IP version preference for filtering IP addresses.</param>
    /// <param name="ipAddressPropertyName">The custom property name for the IP address log property.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        IpVersionPreference ipVersionPreference,
        string ipAddressPropertyName)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new ClientIpEnricher(ipVersionPreference, ipAddressPropertyName));
    }

    /// <summary>
    ///     Registers the correlation id enricher to enrich logs with correlation id with
    ///     'x-correlation-id' header information.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="headerName">
    ///     Set the 'X-Correlation-Id' header in case if service is behind proxy server. Default value
    ///     is 'x-correlation-id'.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///     Add generated correlation id value if correlation id header not available in
    ///     <see cref="HttpContext" /> header collection.
    /// </param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithCorrelationId(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName = "x-correlation-id",
        bool addValueIfHeaderAbsence = false)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new CorrelationIdEnricher(headerName, addValueIfHeaderAbsence));
    }

    /// <summary>
    ///     Registers the HTTP request header enricher to enrich logs with the header value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="propertyName">The property name of log</param>
    /// <param name="headerName">The header name to log its value</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <exception cref="ArgumentNullException">headerName</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithRequestHeader(this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName, string propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));
        ArgumentNullException.ThrowIfNull(headerName, nameof(headerName));

        return enrichmentConfiguration.With(new ClientHeaderEnricher(headerName, propertyName));
    }

    /// <summary>
    ///     Registers the user claims enricher to enrich logs with specified user claim values.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="claimNames">The names of the claims to log.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <exception cref="ArgumentNullException">claimNames</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithUserClaims(this LoggerEnrichmentConfiguration enrichmentConfiguration,
        params string[] claimNames)
    {
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));
        ArgumentNullException.ThrowIfNull(claimNames, nameof(claimNames));

        return enrichmentConfiguration.With(new UserClaimsEnricher(claimNames));
    }
}