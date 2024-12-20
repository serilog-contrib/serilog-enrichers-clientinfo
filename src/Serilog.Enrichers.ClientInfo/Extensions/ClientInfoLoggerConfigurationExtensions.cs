﻿using Microsoft.AspNetCore.Http;
using Serilog.Configuration;
using Serilog.Enrichers;
using System;

namespace Serilog;

/// <summary>
///   Extension methods for setting up client IP, client agent and correlation identifier enrichers <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static class ClientInfoLoggerConfigurationExtensions
{
    /// <summary>
    ///   Registers the client IP enricher to enrich logs with <see cref="Microsoft.AspNetCore.Http.ConnectionInfo.RemoteIpAddress"/> value.
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
        ArgumentNullException.ThrowIfNull(enrichmentConfiguration, nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new CorrelationIdEnricher(headerName, addValueIfHeaderAbsence));
    }

    /// <summary>
    ///   Registers the HTTP request header enricher to enrich logs with the header value.
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
}
