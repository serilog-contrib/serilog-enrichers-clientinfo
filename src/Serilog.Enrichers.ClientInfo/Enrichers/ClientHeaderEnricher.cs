using System;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc />
public class ClientHeaderEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string _headerKey;
    private readonly int _maxLength;
    private readonly string _propertyName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientHeaderEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">The key of the header.</param>
    /// <param name="propertyName">The name of the property.</param>
    public ClientHeaderEnricher(string headerKey, string propertyName)
        : this(headerKey, propertyName, new HttpContextAccessor())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientHeaderEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">The key of the header.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="maxLength">The maximum number of characters of the header value to log.</param>
    public ClientHeaderEnricher(string headerKey, string propertyName, int maxLength)
        : this(headerKey, propertyName, new HttpContextAccessor(), maxLength)
    {
    }

    internal ClientHeaderEnricher(
        string headerKey, 
        string propertyName, 
        IHttpContextAccessor contextAccessor,
        int maxLength = HeaderValueSanitizer.DefaultMaxHeaderLength)
    {
        ArgumentNullException.ThrowIfNull(headerKey, nameof(headerKey));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength, nameof(maxLength));

        _headerKey = headerKey;
        _propertyName = string.IsNullOrWhiteSpace(propertyName) ? headerKey.Replace("-", "") : propertyName;
        _maxLength = maxLength;
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        // The value is read on every event rather than cached in HttpContext.Items: the lookup is cheap and
        // writing to Items is not safe when several threads log within the same request.
        string headerValue = HeaderValueSanitizer.Sanitize(httpContext.Request.Headers[_headerKey].ToString(),
            _maxLength);

        logEvent.AddPropertyIfAbsent(new LogEventProperty(_propertyName, new ScalarValue(headerValue)));
    }
}
