using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Preparers.CorrelationIds;

namespace Serilog.Enrichers;

/// <summary>
///     Extension methods for <see cref="HttpContext" /> to access enriched values.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    ///     Retrieves the correlation ID value from the current HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The correlation ID as a string, or null if not available.</returns>
    public static string GetCorrelationId(this HttpContext httpContext)
        => httpContext?.Items[Constants.CorrelationIdValueKey] as string;

    /// <summary>
    /// Retrieves the correlation ID preparer for processing the current HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>Correlation ID preparer.</returns>
    internal static ICorrelationIdPreparer GetCorrelationIdPreparer(this HttpContext httpContext)
        => httpContext.RequestServices.GetService<ICorrelationIdPreparer>() ?? new CorrelationIdPreparer();
}