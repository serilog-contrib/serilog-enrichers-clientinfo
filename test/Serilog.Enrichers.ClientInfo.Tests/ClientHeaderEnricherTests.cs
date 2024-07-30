﻿using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientHeaderEnricherTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientHeaderEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithClientHeader_WhenHttpRequestContainHeader_ShouldCreateNamedHeaderValueProperty()
    {
        // Arrange
        var headerKey = "RequestId";
        var propertyName = "HttpRequestId";
        var headerValue = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext!.Request!.Headers[headerKey] = headerValue;

        var clientHeaderEnricher = new ClientHeaderEnricher(headerKey, propertyName, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
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
        var headerKey = "RequestId";
        var headerValue = Guid.NewGuid().ToString();
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey] = headerValue;

        var clientHeaderEnricher = new ClientHeaderEnricher(headerKey, propertyName: string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
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
    public void EnrichLogWithMultipleClientHeaderEnricher_WhenHttpRequestContainHeaders_ShouldCreateHeaderValuesProperty()
    {
        // Arrange
        var headerKey1 = "Header1";
        var headerKey2 = "User-Agent";
        var headerValue1 = Guid.NewGuid().ToString();
        var headerValue2 = Guid.NewGuid().ToString();
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey1] = headerValue1;
        _contextAccessor!.HttpContext!.Request!.Headers[headerKey2] = headerValue2;
        var clientHeaderEnricher1 = new ClientHeaderEnricher(headerKey1, propertyName: string.Empty, _contextAccessor);
        var clientHeaderEnricher2 = new ClientHeaderEnricher(headerKey2, propertyName: string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
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
        var headerKey = "RequestId";
        var clientHeaderEnricher = new ClientHeaderEnricher(headerKey, propertyName: string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
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
    public void WithRequestHeader_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestHeader("HeaderName")
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}