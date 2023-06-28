using Serilog.Configuration;
using Serilog.Enrichers;
using System;
using System.Web;

#if NETFULL

using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

namespace Serilog;

/// <summary>
///   Extension methods for setting up client IP, client agent and correlation identifier enrichers <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static class ClientInfoLoggerConfigurationExtensions
{
    /// <summary>
    ///   Registers the client IP enricher to enrich logs with client IP with 'X-forwarded-for'
    ///   header information.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="forwardHeaderName">
    ///   Set the 'X-Forwarded-For' header in case if service is behind proxy server. Default value
    ///   is 'x-forwarded-for'.
    /// </param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string forwardHeaderName = "x-forwarded-for")
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new ClientIpEnricher(forwardHeaderName));
    }

    /// <summary>
    ///   Registers the correlation id enricher to enrich logs with correlation id with
    ///   'x-correlation-id' header information.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="headerName">
    ///   Set the 'X-Correlation-Id' header in case if service is behind proxy server. Default value
    ///   is 'x-correlation-id'.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///   Add generated correlation id value if correlation id header not available in
    ///   <see cref="HttpContext"/> header collection.
    /// </param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithCorrelationId(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName = "x-correlation-id",
        bool addValueIfHeaderAbsence = false)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new CorrelationIdEnricher(headerName, addValueIfHeaderAbsence));
    }

    /// <summary>
    ///   Registers the HTTP request header enricher to enrich logs with the header value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="headerName">The header name to log its value</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <exception cref="ArgumentNullException">headerName</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithRequestHeader(this LoggerEnrichmentConfiguration enrichmentConfiguration, string headerName)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        if (headerName == null)
        {
            throw new ArgumentNullException(nameof(headerName));
        }

        return enrichmentConfiguration.With(new ClientHeaderEnricher(headerName));
    }
}