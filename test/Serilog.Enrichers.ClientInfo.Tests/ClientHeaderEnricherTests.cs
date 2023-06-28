using Microsoft.AspNetCore.Http;
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
    public void EnrichLogWithClientHeaderEnricher_WhenHttpRequestContainHeader_ShouldCreateHeaderValueProperty()
    {
        // Arrange
        var headerKey = "RequestId";
        var headerValue = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers.Add(headerKey, headerValue);

        var clientHeaderEnricher = new ClientHeaderEnricher(headerKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey));
        Assert.Equal(headerValue, evt.Properties[headerKey].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithMulitpleClientHeaderEnricher_WhenHttpRequestContainHeaders_ShouldCreateHeaderValuesProperty()
    {
        // Arrange
        var headerKey1 = "Header1";
        var headerKey2 = "Header2";
        var headerValue1 = Guid.NewGuid().ToString();
        var headerValue2 = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers.Add(headerKey1, headerValue1);
        _contextAccessor.HttpContext.Request.Headers.Add(headerKey2, headerValue2);

        var clientHeaderEnricher1 = new ClientHeaderEnricher(headerKey1, _contextAccessor);
        var clientHeaderEnricher2 = new ClientHeaderEnricher(headerKey2, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher1)
            .Enrich.With(clientHeaderEnricher2)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey1));
        Assert.Equal(headerValue1, evt.Properties[headerKey1].LiteralValue().ToString());
        Assert.True(evt.Properties.ContainsKey(headerKey2));
        Assert.Equal(headerValue2, evt.Properties[headerKey2].LiteralValue().ToString());
    }

    [Fact]
    public void EnrichLogWithClientHeaderEnricher_WhenHttpRequestNotContainHeader_ShouldCreateHeaderValuePropertyWithNoValue()
    {
        // Arrange
        var headerKey = "RequestId";
        var clientHeaderEnricher = new ClientHeaderEnricher(headerKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(headerKey));
        Assert.Null(evt.Properties[headerKey].LiteralValue());
    }
}