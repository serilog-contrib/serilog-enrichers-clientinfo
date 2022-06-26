using Serilog.Configuration;
using Serilog.Enrichers;
using System;

namespace Serilog
{
    /// <summary>
    ///     Extension methods for setting up client IP and client agent enrichers <see cref="LoggerEnrichmentConfiguration"/>.
    /// </summary>
    public static class ClientInfoLoggerConfigurationExtensions
    {
        /// <summary>
        ///     Registers the client IP enricher to enrich logs with client IP with 'X-forwarded-for' header information.
        /// </summary>
        /// <param name="enrichmentConfiguration"> The enrichment configuration. </param>
        /// <param name="xForwardHeaderName">
        ///     Set the 'X-Forwarded-For' header in case if service is behind proxy server. Default value is 'X-forwarded-for'.
        /// </param>
        /// <exception cref="ArgumentNullException"> enrichmentConfiguration </exception>
        /// <returns> The logger configuration so that multiple calls can be chained. </returns>
        public static LoggerConfiguration WithClientIp(this LoggerEnrichmentConfiguration enrichmentConfiguration, string xForwardHeaderName = null)
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            if (!string.IsNullOrEmpty(xForwardHeaderName))
                ClinetIpConfiguration.XForwardHeaderName = xForwardHeaderName;

            return enrichmentConfiguration.With<ClientIpEnricher>();
        }

        /// <summary>
        ///     Registers the client Agent enricher to enrich logs with 'User-Agent' header information.
        /// </summary>
        /// <param name="enrichmentConfiguration"> The enrichment configuration. </param>
        /// <exception cref="ArgumentNullException"> enrichmentConfiguration </exception>
        /// <returns> The logger configuration so that multiple calls can be chained. </returns>
        public static LoggerConfiguration WithClientAgent(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<ClientAgentEnricher>();
        }
    }
}