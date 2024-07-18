using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc/>
public class ClientHeaderEnricher : ILogEventEnricher
{
    private readonly string _clientHeaderItemKey;
    private readonly string _propertyName;
    private readonly string _headerKey;
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ClientHeaderEnricher"/> class.
    /// </summary>
    /// <param name="headerKey">The key of the header.</param>
    /// <param name="propertyName">The name of the property.</param>
    public ClientHeaderEnricher(string headerKey, string propertyName)
        : this(headerKey, propertyName, new HttpContextAccessor())
    {
    }

    internal ClientHeaderEnricher(string headerKey, string propertyName, IHttpContextAccessor contextAccessor)
    {
        _headerKey = headerKey;
        _propertyName = string.IsNullOrWhiteSpace(propertyName)
            ? headerKey.Replace("-", "")
            : propertyName;
        _clientHeaderItemKey = $"Serilog_{headerKey}";
        _contextAccessor = contextAccessor;
    }

    internal ClientHeaderEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (httpContext.Items[_clientHeaderItemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var headerValue = httpContext.Request.Headers[_headerKey].ToString();
        headerValue = string.IsNullOrWhiteSpace(headerValue) ? null : headerValue;

        var logProperty = new LogEventProperty(_propertyName, new ScalarValue(headerValue));
        httpContext.Items.Add(_clientHeaderItemKey, logProperty);

        logEvent.AddPropertyIfAbsent(logProperty);
    }
}