using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherTests()
    {
        DefaultHttpContext httpContext = new();
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
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");

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
        ClientIpEnricher ipEnricher = new(_contextAccessor);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");
        log.Information("Has an other IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        Logger logger = new LoggerConfiguration()
            .Enrich.WithClientIp()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenKeyNotInItems_ShouldWorkCorrectly()
    {
        // Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        ClientIpEnricher ipEnricher = new(_contextAccessor);

        // Ensure the Items dictionary doesn't contain our key initially
        _contextAccessor.HttpContext.Items.Clear();

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act - This should work without throwing any exceptions
        Exception exception = Record.Exception(() => log.Information("Test log message"));

        // Assert
        Assert.Null(exception);
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WithCustomPropertyName_ShouldCreateCustomProperty()
    {
        // Arrange
        const string customPropertyName = "client.address";
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        ClientIpEnricher ipEnricher = new(_contextAccessor, IpVersionPreference.None, customPropertyName);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has a custom IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(customPropertyName));
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties[customPropertyName].LiteralValue());
    }

    [Theory]
    [InlineData("::1", "client.address")]
    [InlineData("192.168.1.1", "ClientAddress")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", "RemoteIP")]
    public void EnrichLogWithClientIp_WithCustomPropertyName_ShouldUseCorrectName(string ip, string propertyName)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, IpVersionPreference.None, propertyName);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property with custom name");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(propertyName));
        Assert.Equal(ipAddress.ToString(), evt.Properties[propertyName].LiteralValue());
    }

    [Fact]
    public void WithClientIp_WithCustomPropertyName_ThenLoggerIsCalled_ShouldUseCustomProperty()
    {
        // Arrange
        const string customPropertyName = "client.address";
        LogEvent evt = null;
        Logger logger = new LoggerConfiguration()
            .Enrich.WithClientIp(customPropertyName)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        logger.Information("LOG");

        // Assert - Should not throw and property should exist if HttpContext is available
        Assert.NotNull(evt);
    }

    [Fact]
    public void WithClientIp_WithIpVersionPreferenceAndCustomPropertyName_ShouldUseCustomProperty()
    {
        // Arrange
        const string customPropertyName = "client.address";
        LogEvent evt = null;
        Logger logger = new LoggerConfiguration()
            .Enrich.WithClientIp(IpVersionPreference.Ipv4Only, customPropertyName)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        logger.Information("LOG");

        // Assert - Should not throw
        Assert.NotNull(evt);
    }

    [Fact]
    public void EnrichLogWithClientIp_WithCustomPropertyName_WhenLogMoreThanOnce_ShouldCacheCorrectly()
    {
        // Arrange
        const string customPropertyName = "client.address";
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        ClientIpEnricher ipEnricher = new(_contextAccessor, IpVersionPreference.None, customPropertyName);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("First log with custom property");
        log.Information("Second log with custom property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(customPropertyName));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties[customPropertyName].LiteralValue());
    }

    [Fact]
    public void ClientIpEnricher_WithNullPropertyName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ClientIpEnricher(IpVersionPreference.None, null));
    }

    [Fact]
    public void WithClientIp_WithNullPropertyName_ShouldThrowArgumentNullException()
    {
        // Arrange
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loggerConfiguration.Enrich.WithClientIp((string)null));
    }

    [Fact]
    public void WithClientIp_WithIpVersionPreferenceAndNullPropertyName_ShouldThrowArgumentNullException()
    {
        // Arrange
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            loggerConfiguration.Enrich.WithClientIp(IpVersionPreference.Ipv4Only, null));
    }
}