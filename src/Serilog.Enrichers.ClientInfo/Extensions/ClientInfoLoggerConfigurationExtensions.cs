using Serilog.Configuration;
using Serilog.Enrichers;
using System;

namespace Serilog
{
    public static class ClientInfoLoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithClientIp(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<ClientIpEnricher>();
        }

        public static LoggerConfiguration WithClientAgent(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<ClientAgentEnricher>();
        }
    }
}