using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <inheritdoc />
public class UserClaimsEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string[] _claimNames;
    private readonly Dictionary<string, string> _claimItemKeys;

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
        _claimItemKeys = new Dictionary<string, string>();

        // Pre-compute item keys for each claim
        foreach (string claimName in _claimNames)
        {
            _claimItemKeys[claimName] = $"Serilog_UserClaim_{claimName}";
        }
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        HttpContext httpContext = _contextAccessor.HttpContext;
        if (httpContext == null) return;

        ClaimsPrincipal user = httpContext.User;
        if (user == null || !user.Identity?.IsAuthenticated == true) return;

        foreach (string claimName in _claimNames)
        {
            string itemKey = _claimItemKeys[claimName];

            // Check if property already exists in HttpContext.Items
            if (httpContext.Items.TryGetValue(itemKey, out object value) &&
                value is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                continue;
            }

            // Get claim value (null if not found)
            string claimValue = user.FindFirst(claimName)?.Value;

            // Create log property with the claim name as the property name
            LogEventProperty claimProperty = new(claimName, new ScalarValue(claimValue));
            httpContext.Items.Add(itemKey, claimProperty);

            logEvent.AddPropertyIfAbsent(claimProperty);
        }
    }
}
