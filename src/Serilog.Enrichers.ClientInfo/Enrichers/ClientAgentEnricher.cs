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
        private const string ClientAgentPropertyName = "ClientAgent";
        private const string ClientAgentItemKey = "Serilog_ClientAgent";

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
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                return;

            if (httpContext.Items[ClientAgentItemKey] is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                return;
            }

#if NETFULL
            var agentName = httpContext.Request.Headers["User-Agent"];
#else
            var agentName = httpContext.Request.Headers["User-Agent"];
#endif


            var clientAgentProperty = new LogEventProperty(ClientAgentPropertyName, new ScalarValue(agentName));
            httpContext.Items.Add(ClientAgentItemKey, clientAgentProperty);

            logEvent.AddPropertyIfAbsent(clientAgentProperty);
        }
    }
}