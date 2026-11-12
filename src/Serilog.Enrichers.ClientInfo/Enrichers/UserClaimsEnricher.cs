using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc />
public class UserClaimsEnricher : ILogEventEnricher
{
    private readonly string[] _claimNames;
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserClaimsEnricher" /> class.
    /// </summary>
    /// <param name="claimNames">The names of the claims to log.</param>
    public UserClaimsEnricher(params string[] claimNames)
        : this(new HttpContextAccessor(), claimNames)
    {
    }

    internal UserClaimsEnricher(IHttpContextAccessor contextAccessor, params string[] claimNames)
    {
        _contextAccessor = contextAccessor;
        _claimNames = claimNames ?? [];
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        ClaimsPrincipal user = httpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // Claims are read from the current principal on every event rather than cached in HttpContext.Items:
        // the principal can be replaced during the request, and writing to Items is not safe when several
        // threads log within the same request.
        foreach (string claimName in _claimNames)
        {
            // Get claim value (null if not found)
            string claimValue = user.FindFirst(claimName)?.Value;

            // Create log property with the claim name as the property name
            logEvent.AddPropertyIfAbsent(new LogEventProperty(claimName, new ScalarValue(claimValue)));
        }
    }
}
