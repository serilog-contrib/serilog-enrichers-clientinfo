using Serilog.Events;
using System.Net;
using Serilog;
using Serilog.Enrichers;
using Serilog.Enrichers.ClientInfo.Tests;
using Xunit;

#if NETFULL

using Serilog.Enrichers.ClientInfo.Accessors;

#else
using Microsoft.AspNetCore.Http;
#endif

public class ClientIpEnricherTests : TestBase
{
    private const string ForwardHeaderKey = "x-forwarded-for";
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherTests()
    {
        _contextAccessor = GetMockHttpContextAccessor;
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("192.168.1.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        // Arrange
        //var ipAddress = IPAddress.Parse(ip);
        //_contextAccessor.HttpContext.Connection.RemoteIpAddress = ipAddress;
        SetIp(ip);

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
        Assert.Equal(ip, evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenLogMoreThanOnce_ShouldReadClientIpValueFromHttpContextItems()
    {
        //Arrange
        //_contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        var ip = IPAddress.Loopback.ToString();
        SetIp(ip);
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
        Assert.Equal(ip, evt.Properties["ClientIp"].LiteralValue());
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("192.168.1.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_WhenRequestContainForwardHeader_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        //Arrange
        SetIp(ip);
        _contextAccessor.HttpContext.Request.Headers.Add(ForwardHeaderKey, ip);

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
        Assert.Equal(ip, evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WithCustomForwardHeaderAndRequest_ShouldCreateClientIpPropertyWithValue()
    {
        //Arrange
        const string customForwardHeader = "CustomForwardHeader";
        var ip = IPAddress.Loopback.ToString();
        SetIp(ip);

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

    private void SetIp(string ip)
    {
#if NETFULL
        _contextAccessor.HttpContext.Request.ServerVariables["REMOTE_ADDR"] = ip;
#else
    var ipAddress = IPAddress.Parse(ip);
    _contextAccessor.HttpContext.Connection.RemoteIpAddress = ipAddress;
#endif
    }
}