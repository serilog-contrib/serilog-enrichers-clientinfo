using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class CorrelationIdEnricherTests
{
    private const string HeaderKey = "x-correlation-id";
    private const string LogPropertyName = "CorrelationId";
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
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldCreateCorrelationIdPropertyHasValue()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Null(evt.Properties[LogPropertyName].LiteralValue());
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.NotNull(evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpResponseContainsCorrelationIdHeader_ShouldCreateCorrelationIdProperty()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(correlationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithCorrelationId_WhenHttpRequestAndResponseContainCorrelationIdHeader_ShouldCreateCorrelationIdPropertyFromHttpRequest()
    {
        // Arrange
        var requestCorrelationId = Guid.NewGuid().ToString();
        var responseCorrelationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = requestCorrelationId;
        _contextAccessor.HttpContext!.Response!.Headers[HeaderKey] = responseCorrelationId;
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
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(requestCorrelationId, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithCorrelationId()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}