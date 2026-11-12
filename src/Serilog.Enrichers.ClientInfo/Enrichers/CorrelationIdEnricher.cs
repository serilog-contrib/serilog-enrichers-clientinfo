using System;
using System.Collections.Generic;
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
    private readonly bool _addValueIfHeaderAbsence;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string _headerKey;
    private readonly int _maxLength;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorrelationIdEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">
    ///     The header key used to retrieve the correlation ID from the HTTP request or response headers.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///     Determines whether to add a new correlation ID value if the header is absent.
    /// </param>
    public CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence)
        : this(headerKey, addValueIfHeaderAbsence, new HttpContextAccessor())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorrelationIdEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">
    ///     The header key used to retrieve the correlation ID from the HTTP request or response headers.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///     Determines whether to add a new correlation ID value if the header is absent or not acceptable.
    /// </param>
    /// <param name="maxLength">
    ///     The maximum accepted length of a correlation ID header value. Longer values are treated as absent.
    /// </param>
    public CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence, int maxLength)
        : this(headerKey, addValueIfHeaderAbsence, new HttpContextAccessor(), maxLength)
    {
    }

    internal CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence, IHttpContextAccessor contextAccessor,
        int maxLength = HeaderValueSanitizer.DefaultMaxCorrelationIdLength)
    {
        ArgumentNullException.ThrowIfNull(headerKey, nameof(headerKey));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength, nameof(maxLength));

        _headerKey = headerKey;
        _addValueIfHeaderAbsence = addValueIfHeaderAbsence;
        _contextAccessor = contextAccessor;
        _maxLength = maxLength;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        LogEventProperty correlationIdProperty;
        bool created = false;

        // The correlation id must stay the same for the whole request, so it is kept in HttpContext.Items.
        // Items is not thread-safe, and the dictionary itself is created lazily on first access, so the lock
        // is taken on the HttpContext: it makes the check-and-store atomic when several threads log within
        // the same request, so they all see one id.
        lock (httpContext)
        {
            IDictionary<object, object> items = httpContext.Items;

            if (items.TryGetValue(CorrelationIdItemKey, out object value) &&
                value is LogEventProperty cachedProperty)
            {
                correlationIdProperty = cachedProperty;
            }
            else
            {
                string correlationId = ResolveCorrelationId(httpContext);
                correlationIdProperty = new LogEventProperty(PropertyName, new ScalarValue(correlationId));

                items[CorrelationIdItemKey] = correlationIdProperty;
                items[Constants.CorrelationIdValueKey] = correlationId;
                created = true;
            }
        }

        if (created)
            logEvent.AddOrUpdateProperty(correlationIdProperty);
        else
            logEvent.AddPropertyIfAbsent(correlationIdProperty);
    }

    private string ResolveCorrelationId(HttpContext httpContext)
    {
        // The request header is client-controlled: a value that is too long or contains control characters
        // is treated as absent instead of being logged and handed to the application.
        string correlationId = GetAcceptableValue(httpContext.Request.Headers[_headerKey])
                               ?? GetAcceptableValue(httpContext.Response.Headers[_headerKey]);

        if (correlationId == null && _addValueIfHeaderAbsence)
            correlationId = Guid.NewGuid().ToString();

        return correlationId;
    }

    private string GetAcceptableValue(StringValues header)
    {
        string value = header.ToString();
        return HeaderValueSanitizer.IsValid(value, _maxLength) ? value : null;
    }
}
