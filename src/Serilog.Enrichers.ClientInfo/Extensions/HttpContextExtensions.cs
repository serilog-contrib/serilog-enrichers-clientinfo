using Microsoft.AspNetCore.Http;

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
}