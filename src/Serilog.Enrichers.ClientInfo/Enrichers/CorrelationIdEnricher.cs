using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using Serilog.Preparers.CorrelationIds;

#nullable enable
namespace Serilog.Enrichers;

/// <inheritdoc />
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdItemKey = "Serilog_CorrelationId";
    private const string PropertyName = "CorrelationId";
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly CorrelationIdPreparerOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CorrelationIdEnricher" /> class.
    /// </summary>
    /// <param name="headerKey">
    ///     The header key used to retrieve the correlation ID from the HTTP request or response headers.
    /// </param>
    /// <param name="addValueIfHeaderAbsence">
    ///     Determines whether to add a new correlation ID value if the header is absent.
    /// </param>
    public CorrelationIdEnricher(
        string headerKey,
        bool addValueIfHeaderAbsence)
        : this(
              headerKey,
              addValueIfHeaderAbsence,
              new HttpContextAccessor())
    {
    }

    internal CorrelationIdEnricher(
        string headerKey,
        bool addValueIfHeaderAbsence,
        IHttpContextAccessor contextAccessor)
    {
        _options = new CorrelationIdPreparerOptions(addValueIfHeaderAbsence, headerKey);
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out object? value) &&
            value is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);

            // Ensure the string value is also available if not already stored
            if (!httpContext.Items.ContainsKey(Constants.CorrelationIdValueKey))
            {
                string? correlationIdValue = ((ScalarValue)logEventProperty.Value).Value as string;
                httpContext.Items.Add(Constants.CorrelationIdValueKey, correlationIdValue);
            }

            return;
        }

        ICorrelationIdPreparer correlationIdPreparer = httpContext.GetCorrelationIdPreparer();

        string? correlationId = correlationIdPreparer.PrepareCorrelationId(httpContext, _options);

        LogEventProperty correlationIdProperty = new(PropertyName, new ScalarValue(correlationId));
        logEvent.AddOrUpdateProperty(correlationIdProperty);

        httpContext.Items.Add(CorrelationIdItemKey, correlationIdProperty);
        httpContext.Items.Add(Constants.CorrelationIdValueKey, correlationId);
    }
}
#nullable disable