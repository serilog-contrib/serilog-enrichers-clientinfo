using Serilog.Core;
using Serilog.Events;

#if NETFULL

using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

namespace Serilog.Enrichers
{
    public class ClientIpEnricher : ILogEventEnricher
    {
        private const string IpAddressPropertyName = "ClientIp";
        private readonly IHttpContextAccessor _contextAccessor;

        public ClientIpEnricher()
        {
            _contextAccessor = new HttpContextAccessor();
        }

        internal ClientIpEnricher(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_contextAccessor.HttpContext == null)
                return;

            var ipAddress = GetIpAddress();

            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "unknown";

            var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));

            logEvent.AddPropertyIfAbsent(ipAddressProperty);
        }

#if NETFULL

        private string GetIpAddress()
        {
            var ipAddress = _contextAccessor.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    return addresses[0];
            }

            return _contextAccessor.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

#else
     private string GetIpAddress()
     {
        return _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
     }
#endif
    }
}