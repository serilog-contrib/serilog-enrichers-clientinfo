using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherTests
{
    private const string ForwardHeaderKey = "x-forwarded-for";
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
    public void EnrichLogWithClientIp_ShouldCreateClientIpPropertyWithValue()
    {
        // Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("::1");

        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("::1", evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenLogMoreThanOnce_ShouldReadClientIpValueFromHttpContextItems()
    {
        //Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");
        log.Information(@"Has an other IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenRequestContainForwardHeader_ShouldCreateClientIpPropertyWithValue()
    {
        //Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers.Add(ForwardHeaderKey, IPAddress.Broadcast.ToString());

        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Broadcast.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WithCustomForwardHeaderAndRequest_ShouldCreateClientIpPropertyWithValue()
    {
        //Arrange
        const string customForwardHeader = "CustomForwardHeader";
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers.Add(customForwardHeader, IPAddress.Broadcast.ToString());

        var ipEnricher = new ClientIpEnricher(customForwardHeader, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Broadcast.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithClientIp()
            .WriteTo.Sink(new DelegatingSink(e => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }
}