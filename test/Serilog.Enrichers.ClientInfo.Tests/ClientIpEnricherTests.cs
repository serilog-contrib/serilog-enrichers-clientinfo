using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("192.168.1.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        // Arrange
        var ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        var ipEnricher = new ClientIpEnricher(_contextAccessor);

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
        Assert.Equal(ipAddress.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenLogMoreThanOnce_ShouldReadClientIpValueFromHttpContextItems()
    {
        //Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        var ipEnricher = new ClientIpEnricher(_contextAccessor);

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
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithClientIp()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenKeyNotInItems_ShouldWorkCorrectly()
    {
        // Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        var ipEnricher = new ClientIpEnricher(_contextAccessor);
        
        // Ensure the Items dictionary doesn't contain our key initially
        _contextAccessor.HttpContext.Items.Clear();
        
        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act - This should work without throwing any exceptions
        var exception = Record.Exception(() => log.Information("Test log message"));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }
}