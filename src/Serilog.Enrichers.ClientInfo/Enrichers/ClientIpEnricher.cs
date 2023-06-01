using Serilog.Core;
using Serilog.Events;
using System.Linq;
using System.Runtime.CompilerServices;

#if NETFULL

using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

[assembly: InternalsVisibleTo("Serilog.Enrichers.ClientInfo.Tests")]

namespace Serilog.Enrichers
{
    public class ClientIpEnricher : ILogEventEnricher
    {
        private const string IpAddressPropertyName = "ClientIp";
        private const string IpAddressItemKey = "Serilog_ClientIp";

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
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                return;

            if (httpContext.Items[IpAddressItemKey] is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                return;
            }

            var ipAddress = GetIpAddress();

            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "unknown";

            var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));
            httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);

            logEvent.AddPropertyIfAbsent(ipAddressProperty);
        }

#if NETFULL

        private string GetIpAddress()
        {
            var ipAddress = _contextAccessor.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            return !string.IsNullOrEmpty(ipAddress)
                ? GetIpAddressFromProxy(ipAddress)
                : _contextAccessor.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }

#else
        private string GetIpAddress()
        {
            var ipAddress = _contextAccessor.HttpContext?.Request?.Headers[ClinetIpConfiguration.XForwardHeaderName].FirstOrDefault();

            return !string.IsNullOrEmpty(ipAddress)
                ? GetIpAddressFromProxy(ipAddress)
                : _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
#endif

        private string GetIpAddressFromProxy(string proxifiedIpList)
        {
            var addresses = proxifiedIpList.Split(',');

            return addresses.Length == 0 ? string.Empty : addresses[0].Trim();
        }
    }
}