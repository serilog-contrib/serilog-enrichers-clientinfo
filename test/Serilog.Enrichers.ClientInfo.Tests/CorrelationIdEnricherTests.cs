﻿using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Enrichers;
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

    [Fact]
    public void GetCorrelationId_WhenHttpRequestContainCorrelationHeader_ShouldReturnCorrelationIdFromHttpContext()
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
        var retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(correlationId, retrievedCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsTrue_ShouldReturnGeneratedCorrelationIdFromHttpContext()
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
        var retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.NotNull(retrievedCorrelationId);
        Assert.NotEmpty(retrievedCorrelationId);
        // Verify it's a valid GUID format
        Assert.True(Guid.TryParse(retrievedCorrelationId, out _));
    }

    [Fact]
    public void GetCorrelationId_WhenHttpRequestNotContainCorrelationHeaderAndAddDefaultValueIsFalse_ShouldReturnNullFromHttpContext()
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
        var retrievedCorrelationId = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Null(retrievedCorrelationId);
    }

    [Fact]
    public void GetCorrelationId_WhenCalledMultipleTimes_ShouldReturnSameCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[HeaderKey] = correlationId;
        var correlationIdEnricher = new CorrelationIdEnricher(HeaderKey, false, _contextAccessor);

        var log = new LoggerConfiguration()
            .Enrich.With(correlationIdEnricher)
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        log.Information(@"First log message.");
        var firstRetrieval = _contextAccessor.HttpContext!.GetCorrelationId();
        
        log.Information(@"Second log message.");
        var secondRetrieval = _contextAccessor.HttpContext!.GetCorrelationId();

        // Assert
        Assert.Equal(correlationId, firstRetrieval);
        Assert.Equal(correlationId, secondRetrieval);
        Assert.Equal(firstRetrieval, secondRetrieval);
    }

    [Fact]
    public void GetCorrelationId_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange & Act
        var result = HttpContextExtensions.GetCorrelationId(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void EnrichLogWithCorrelationId_BackwardCompatibility_OldRetrievalMethodShouldStillWork()
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

        // Test that the old way (hacky way) still works
        var httpContext = _contextAccessor.HttpContext!;
        string retrievedCorrelationIdOldWay = null;
        
        if (httpContext.Items.TryGetValue("Serilog_CorrelationId", out var correlationIdItem) &&
            correlationIdItem is LogEventProperty { Name: "CorrelationId" } correlationIdProperty)
        {
            retrievedCorrelationIdOldWay = ((ScalarValue)correlationIdProperty.Value).Value as string;
        }

        // Test that the new way also works
        var retrievedCorrelationIdNewWay = httpContext.GetCorrelationId();

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(correlationId, retrievedCorrelationIdOldWay);
        Assert.Equal(correlationId, retrievedCorrelationIdNewWay);
        Assert.Equal(retrievedCorrelationIdOldWay, retrievedCorrelationIdNewWay);
    }
}