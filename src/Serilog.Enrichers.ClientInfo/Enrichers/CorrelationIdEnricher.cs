using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc />
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdItemKey = "Serilog_CorrelationId";
    private const string PropertyName = "CorrelationId";
    private readonly bool _addCorrelationIdToResponse;
    private readonly bool _addValueIfHeaderAbsence;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string _headerKey;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorrelationIdEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">
    ///     The header key used to retrieve the correlation ID from the HTTP request or response headers.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///     Determines whether to add a new correlation ID value if the header is absent.
    /// </param>
    /// <param name="addCorrelationIdToResponse">
    ///     Determines whether to add correlation ID value to <see cref="HttpContext.Response" /> header collection.
    /// </param>
    public CorrelationIdEnricher(
        string headerKey,
        bool addValueIfHeaderAbsence,
        bool addCorrelationIdToResponse)
        : this(
              headerKey,
              addValueIfHeaderAbsence,
              addCorrelationIdToResponse,
              new HttpContextAccessor())
    {
    }

    internal CorrelationIdEnricher(
        string headerKey,
        bool addValueIfHeaderAbsence,
        bool addCorrelationIdToResponse,
        IHttpContextAccessor contextAccessor)
    {
        _headerKey = headerKey;
        _addValueIfHeaderAbsence = addValueIfHeaderAbsence;
        _addCorrelationIdToResponse = addCorrelationIdToResponse;
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out object value) &&
            value is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);

            // Ensure the string value is also available if not already stored
            if (!httpContext.Items.ContainsKey(Constants.CorrelationIdValueKey))
            {
                string correlationIdValue = ((ScalarValue)logEventProperty.Value).Value as string;
                httpContext.Items.Add(Constants.CorrelationIdValueKey, correlationIdValue);
            }

            return;
        }

        string correlationId = PrepareCorrelationId(httpContext);

        AddCorrelationIdToResponse(httpContext, correlationId);

        LogEventProperty correlationIdProperty = new(PropertyName, new ScalarValue(correlationId));
        logEvent.AddOrUpdateProperty(correlationIdProperty);

        httpContext.Items.Add(CorrelationIdItemKey, correlationIdProperty);
        httpContext.Items.Add(Constants.CorrelationIdValueKey, correlationId);
    }

    private void AddCorrelationIdToResponse(HttpContext httpContext, string correlationId)
    {
        if (_addCorrelationIdToResponse
            && !httpContext.Response.Headers.ContainsKey(_headerKey))
        {
            httpContext.Response.Headers[_headerKey] = correlationId;
        }
    }

    private string PrepareCorrelationId(HttpContext httpContext)
    {
        StringValues requestHeader = httpContext.Request.Headers[_headerKey];

        if (!string.IsNullOrWhiteSpace(requestHeader))
        {
            return requestHeader;
        }

        StringValues responseHeader = httpContext.Response.Headers[_headerKey];

        if (!string.IsNullOrWhiteSpace(responseHeader))
        {
            return responseHeader;
        }

        if (_addValueIfHeaderAbsence)
        {
            return Guid.NewGuid().ToString();
        }

        return null;
    }
}