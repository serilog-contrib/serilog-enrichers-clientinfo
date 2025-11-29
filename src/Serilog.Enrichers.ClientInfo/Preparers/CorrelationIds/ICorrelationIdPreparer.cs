using Microsoft.AspNetCore.Http;

#nullable enable
namespace Serilog.Preparers.CorrelationIds
{
    /// <summary>
    /// Preparer for correlation ID.
    /// </summary>
    public interface ICorrelationIdPreparer
    {
        /// <summary>
        /// Prepares the correlation ID.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="correlationIdPreparerOptions">Options for preparation.</param>
        /// <returns>The correlation ID.</returns>
        string? PrepareCorrelationId(
            HttpContext httpContext,
            CorrelationIdPreparerOptions correlationIdPreparerOptions);
    }
}
#nullable disable