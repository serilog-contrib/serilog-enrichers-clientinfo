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
    public void EnrichLogWithClientHeader_WhenHeaderValueExceedsMaxLength_ShouldTruncateValue()
    {
        // Arrange
        string headerKey = "RequestId";
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = new string('a', 50);
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor, 10);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(new string('a', 10), evt.Properties[headerKey].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHeaderValueExceedsDefaultMaxLength_ShouldTruncateTo1024()
    {
        // Arrange
        string headerKey = "RequestId";
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = new string('a', 30000);
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(1024, ((string)evt.Properties[headerKey].LiteralValue()).Length);
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHeaderValueContainsControlCharacters_ShouldRemoveThem()
    {
        // Arrange
        string headerKey = "RequestId";
        string lineFeed = ((char)0x0A).ToString();
        string carriageReturn = ((char)0x0D).ToString();
        string lineSeparator = ((char)0x2028).ToString();
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] =
            "value" + carriageReturn + lineFeed + "[12:00:00] INF forged" + lineSeparator + "entry";
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal("value[12:00:00] INF forgedentry", evt.Properties[headerKey].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHeaderValueContainsOnlyControlCharacters_ShouldLogNull()
    {
        // Arrange
        string headerKey = "RequestId";
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = new string((char)0x00, 3);
        ClientHeaderEnricher clientHeaderEnricher = new(headerKey, string.Empty, _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.Null(evt.Properties[headerKey].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenSameHeaderIsLoggedUnderTwoNames_ShouldCreateBothProperties()
    {
        // Arrange
        string headerKey = "User-Agent";
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = "agent";
        ClientHeaderEnricher first = new(headerKey, "FirstName", _contextAccessor);
        ClientHeaderEnricher second = new(headerKey, "SecondName", _contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(first)
            .Enrich.With(second)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal("agent", evt.Properties["FirstName"].LiteralValue());
        Assert.Equal("agent", evt.Properties["SecondName"].LiteralValue());
    }

    [Fact]
    public void WithRequestHeader_WithNonPositiveMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LoggerConfiguration loggerConfiguration = new();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            loggerConfiguration.Enrich.WithRequestHeader("HeaderName", null, 0));
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