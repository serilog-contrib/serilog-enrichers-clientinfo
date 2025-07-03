using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Serilog.Enrichers;

/// <inheritdoc/>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdItemKey = "Serilog_CorrelationId";
    private const string CorrelationIdValueKey = "Serilog_CorrelationId_Value";
    private const string PropertyName = "CorrelationId";
    private readonly string _headerKey;
    private readonly bool _addValueIfHeaderAbsence;
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CorrelationIdEnricher"/> class.
    /// </summary>
    /// <param name="headerKey">
    ///   The header key used to retrieve the correlation ID from the HTTP request or response headers.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///   Determines whether to add a new correlation ID value if the header is absent.
    /// </param>
    public CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence)
        : this(headerKey, addValueIfHeaderAbsence, new HttpContextAccessor())
    {
    }

    internal CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence, IHttpContextAccessor contextAccessor)
    {
        _headerKey = headerKey;
        _addValueIfHeaderAbsence = addValueIfHeaderAbsence;
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (httpContext.Items[CorrelationIdItemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            
            // Ensure the string value is also available if not already stored
            if (!httpContext.Items.ContainsKey(CorrelationIdValueKey))
            {
                var correlationIdValue = ((ScalarValue)logEventProperty.Value).Value as string;
                httpContext.Items.Add(CorrelationIdValueKey, correlationIdValue);
            }
            
            return;
        }

        var requestHeader = httpContext.Request.Headers[_headerKey];
        var responseHeader = httpContext.Response.Headers[_headerKey];

        string correlationId;

        if (!string.IsNullOrWhiteSpace(requestHeader))
        {
            correlationId = requestHeader;
        }
        else if (!string.IsNullOrWhiteSpace(responseHeader))
        {
            correlationId = responseHeader;
        }
        else if (_addValueIfHeaderAbsence)
        {
            correlationId = Guid.NewGuid().ToString();
        }
        else
        {
            correlationId = null;
        }

        var correlationIdProperty = new LogEventProperty(PropertyName, new ScalarValue(correlationId));
        logEvent.AddOrUpdateProperty(correlationIdProperty);

        httpContext.Items.Add(CorrelationIdItemKey, correlationIdProperty);
        httpContext.Items.Add(CorrelationIdValueKey, correlationId);
    }
}