using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientHeaderEnricherTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientHeaderEnricherTests()
    {
        DefaultHttpContext httpContext = new();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHttpRequestContainHeader_ShouldCreateNamedHeaderValueProperty()
    {
        // Arrange
        string headerKey = "RequestId";
        string propertyName = "HttpRequestId";
        string headerValue = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = headerValue;

        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, propertyName, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("First testing log enricher.");
        log.Information("Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(propertyName));
        Assert.Equal(headerValue, evt.Properties[propertyName].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHttpRequestContainHeader_ShouldCreateHeaderValueProperty()
    {
        // Arrange
        string headerKey = "RequestId";
        string headerValue = Guid.NewGuid().ToString();
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey] = headerValue;

        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("First testing log enricher.");
        log.Information("Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey));
        Assert.Equal(headerValue, evt.Properties[headerKey].LiteralValue().ToString());
    }

    [Fact]
    public void
        EnrichLogWithMultipleClientHeaderEnricher_WhenHttpRequestContainHeaders_ShouldCreateHeaderValuesProperty()
    {
        // Arrange
        string headerKey1 = "Header1";
        string headerKey2 = "User-Agent";
        string headerValue1 = Guid.NewGuid().ToString();
        string headerValue2 = Guid.NewGuid().ToString();
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey1] = headerValue1;
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey2] = headerValue2;
        ClientHeaderEnricher clientHeaderEnricher1 = new(headerKey1, string.Empty, _contextAccessor);
        ClientHeaderEnricher clientHeaderEnricher2 = new(headerKey2, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher1)
            .Enrich.With(clientHeaderEnricher2)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("First testing log enricher.");
        log.Information("Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey1));
        Assert.Equal(headerValue1, evt.Properties[headerKey1].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(headerKey2.Replace("-", "")));
        Assert.Equal(headerValue2, evt.Properties[headerKey2.Replace("-", "")].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHttpRequestNotContainHeader_ShouldCreateHeaderValuePropertyWithNoValue()
    {
        // Arrange
        string headerKey = "RequestId";
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("First testing log enricher.");
        log.Information("Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey));
        Assert.Null(evt.Properties[headerKey].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenKeyNotInItems_ShouldWorkCorrectly()
    {
        // Arrange
        string headerKey = "x-dummy-header";
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("Testing log enricher.");

        // Act - This should work without throwing any exceptions
        Exception exception = Record.Exception(() => log.Information("Test log message"));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(evt);
    }

    [Fact]
    public void WithRequestHeader_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        Logger logger = new LoggerConfiguration()
            .Enrich.WithRequestHeader("HeaderName")
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}