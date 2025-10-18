using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Security.Claims;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class UserClaimsEnricherTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserClaimsEnricherTests()
    {
        DefaultHttpContext httpContext = new();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenUserIsAuthenticated_ShouldCreateClaimProperties()
    {
        // Arrange
        string userId = "user123";
        string userEmail = "user@example.com";

        ClaimsIdentity identity = new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail)
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, ClaimTypes.NameIdentifier, ClaimTypes.Email);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.NameIdentifier));
        Assert.Equal(userId, evt.Properties[ClaimTypes.NameIdentifier].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.Email));
        Assert.Equal(userEmail, evt.Properties[ClaimTypes.Email].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenClaimDoesNotExist_ShouldLogNullValue()
    {
        // Arrange
        string userId = "user123";

        ClaimsIdentity identity = new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, ClaimTypes.NameIdentifier, ClaimTypes.Email);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.NameIdentifier));
        Assert.Equal(userId, evt.Properties[ClaimTypes.NameIdentifier].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.Email));
        Assert.Null(evt.Properties[ClaimTypes.Email].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenUserIsNotAuthenticated_ShouldNotAddProperties()
    {
        // Arrange
        ClaimsIdentity identity = new(); // Not authenticated
        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, ClaimTypes.NameIdentifier);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenUserIsNull_ShouldNotAddProperties()
    {
        // Arrange
        _contextAccessor.HttpContext!.User = null;

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, ClaimTypes.NameIdentifier);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenHttpContextIsNull_ShouldNotThrow()
    {
        // Arrange
        IHttpContextAccessor nullContextAccessor = Substitute.For<IHttpContextAccessor>();
        nullContextAccessor.HttpContext.Returns((HttpContext)null);

        UserClaimsEnricher userClaimsEnricher = new(nullContextAccessor, ClaimTypes.NameIdentifier);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => log.Information("Test log message."));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(evt);
    }

    [Fact]
    public void EnrichLogWithUserClaims_WhenCalledMultipleTimes_ShouldUseCachedValue()
    {
        // Arrange
        string userId = "user123";

        ClaimsIdentity identity = new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, ClaimTypes.NameIdentifier);

        LogEvent evt1 = null;
        LogEvent evt2 = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => { evt1 ??= e; evt2 = e; }))
            .CreateLogger();

        // Act
        log.Information("First log message.");
        log.Information("Second log message.");

        // Assert
        Assert.NotNull(evt1);
        Assert.NotNull(evt2);
        Assert.True(evt1.Properties.ContainsKey(ClaimTypes.NameIdentifier));
        Assert.True(evt2.Properties.ContainsKey(ClaimTypes.NameIdentifier));
        Assert.Equal(userId, evt1.Properties[ClaimTypes.NameIdentifier].LiteralValue().ToString());
        Assert.Equal(userId, evt2.Properties[ClaimTypes.NameIdentifier].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithUserClaims_WithMultipleClaims_ShouldLogAllSpecifiedClaims()
    {
        // Arrange
        string userId = "user123";
        string userEmail = "user@example.com";
        string userName = "John Doe";
        string userRole = "Admin";

        ClaimsIdentity identity = new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, userRole)
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor,
            ClaimTypes.NameIdentifier,
            ClaimTypes.Email,
            ClaimTypes.Name,
            ClaimTypes.Role);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.NameIdentifier));
        Assert.Equal(userId, evt.Properties[ClaimTypes.NameIdentifier].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.Email));
        Assert.Equal(userEmail, evt.Properties[ClaimTypes.Email].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.Name));
        Assert.Equal(userName, evt.Properties[ClaimTypes.Name].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(ClaimTypes.Role));
        Assert.Equal(userRole, evt.Properties[ClaimTypes.Role].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithUserClaims_WithEmptyClaimArray_ShouldNotAddProperties()
    {
        // Arrange
        ClaimsIdentity identity = new(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.DoesNotContain(ClaimTypes.NameIdentifier, evt.Properties.Keys);
    }

    [Fact]
    public void WithUserClaims_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        Logger logger = new LoggerConfiguration()
            .Enrich.WithUserClaims(ClaimTypes.NameIdentifier)
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnrichLogWithUserClaims_WithCustomClaimType_ShouldLogCustomClaim()
    {
        // Arrange
        string customClaimType = "custom_claim_type";
        string customClaimValue = "custom_value";

        ClaimsIdentity identity = new(new[]
        {
            new Claim(customClaimType, customClaimValue)
        }, "TestAuth");

        _contextAccessor.HttpContext!.User = new ClaimsPrincipal(identity);

        UserClaimsEnricher userClaimsEnricher = new(_contextAccessor, customClaimType);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(userClaimsEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Test log message.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(customClaimType));
        Assert.Equal(customClaimValue, evt.Properties[customClaimType].LiteralValue().ToString());
    }
}
