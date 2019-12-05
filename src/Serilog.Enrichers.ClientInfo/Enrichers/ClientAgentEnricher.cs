using Serilog.Core;
using Serilog.Events;

#if NETFULL

using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

namespace Serilog.Enrichers
{
    public class ClientAgentEnricher : ILogEventEnricher
    {
        private const string IpAddressPropertyName = "ClientAgent";
        private readonly IHttpContextAccessor _contextAccessor;

        public ClientAgentEnricher() : this(new HttpContextAccessor())
        {
        }

        internal ClientAgentEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_contextAccessor.HttpContext == null)
                return;

            var agentName = _contextAccessor.HttpContext.Request.Headers["User-Agent"];

            var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(agentName ?? "unknown"));

            logEvent.AddPropertyIfAbsent(ipAddressProperty);
        }
    }
}