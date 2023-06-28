using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class CorrelationIdEnricherTests
{
    private const string HeaderKey = "x-correlation-id";
    private readonly IHttpContextAccessor _contextAccessor;

    public CorrelationIdEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldCreateCorrelationIdProperty()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers.Add(HeaderKey, correlationId);

        var correlationIdEnricher = new CorrelationIdEnricher(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(HeaderKey));
        Assert.Equal(correlationId, evt.Properties[HeaderKey].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldCreateCorrelationIdPropertyHasValue()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers.Add(HeaderKey, correlationId);

        var correlationIdEnricher = new CorrelationIdEnricher(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(HeaderKey));
        Assert.Equal(correlationId, evt.Properties[HeaderKey].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsFalse_ShouldCreateCorrelationIdPropertyWithNoValue()
    {
        // Arrange
        var correlationIdEnricher = new CorrelationIdEnricher(HeaderKey, false, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(HeaderKey));
        Assert.Null(evt.Properties[HeaderKey].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsTrue_ShouldCreateCorrelationIdPropertyHasValue()
    {
        // Arrange
        var correlationIdEnricher = new CorrelationIdEnricher(HeaderKey, true, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a correlation id.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(HeaderKey));
        Assert.NotNull(evt.Properties[HeaderKey].LiteralValue().ToString());
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithCorrelationId()
            .WriteTo.Sink(new DelegatingSink(e => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}