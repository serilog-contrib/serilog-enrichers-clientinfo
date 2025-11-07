using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Serilog.Preparers.CorrelationIds
{
    internal class CorrelationIdPreparer : ICorrelationIdPreparer
    {
        protected AsyncLocal<string?> CorrelationId { get; } = new AsyncLocal<string?>();

        /// <inheritdoc/>
        public string? PrepareCorrelationId(
            HttpContext httpContext,
            CorrelationIdPreparerOptions correlationIdPreparerOptions)
        {
            if (string.IsNullOrEmpty(CorrelationId.Value))
            {
                CorrelationId.Value = PrepareValueForCorrelationId(httpContext, correlationIdPreparerOptions);
            }

            return CorrelationId.Value;
        }

        protected string? PrepareValueForCorrelationId(
            HttpContext httpContext,
            CorrelationIdPreparerOptions correlationIdPreparerOptions)
        {
            StringValues requestHeader = httpContext.Request.Headers[correlationIdPreparerOptions.HeaderKey];

            if (!string.IsNullOrWhiteSpace(requestHeader))
            {
                return requestHeader;
            }

            StringValues responseHeader = httpContext.Response.Headers[correlationIdPreparerOptions.HeaderKey];

            if (!string.IsNullOrWhiteSpace(responseHeader))
            {
                return responseHeader;
            }

            if (correlationIdPreparerOptions.AddValueIfHeaderAbsence)
            {
                return Guid.NewGuid().ToString();
            }

            return null;
        }
    }
}
#nullable disable