using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherIpVersionPreferenceTests
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherIpVersionPreferenceTests()
    {
        DefaultHttpContext httpContext = new();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Theory]
    [InlineData("192.168.1.1", IpVersionPreference.None, "192.168.1.1")]
    [InlineData("::1", IpVersionPreference.None, "::1")]
    [InlineData("192.168.1.1", IpVersionPreference.PreferIpv4, "192.168.1.1")]
    [InlineData("::1", IpVersionPreference.PreferIpv6, "::1")]
    public void EnrichLogWithClientIp_WithNoneOrPreferenceForSingleIp_ShouldLogTheIp(string ip,
        IpVersionPreference preference, string expectedIp)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, preference);

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
        Assert.Equal(expectedIp, evt.Properties["ClientIp"].LiteralValue());
    }

    [Theory]
    [InlineData("192.168.1.1", IpVersionPreference.Ipv4Only)]
    [InlineData("127.0.0.1", IpVersionPreference.Ipv4Only)]
    public void EnrichLogWithClientIp_WithIpv4OnlyPreferenceAndIpv4Address_ShouldLogTheIp(string ip,
        IpVersionPreference preference)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, preference);

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
        Assert.Equal(ip, evt.Properties["ClientIp"].LiteralValue());
    }

    [Theory]
    [InlineData("::1", IpVersionPreference.Ipv6Only)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", IpVersionPreference.Ipv6Only)]
    public void EnrichLogWithClientIp_WithIpv6OnlyPreferenceAndIpv6Address_ShouldLogTheIp(string ip,
        IpVersionPreference preference)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, preference);

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
        // Compare the parsed IP address toString result directly instead of the input string
        Assert.Equal(ipAddress.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Theory]
    [InlineData("::1", IpVersionPreference.Ipv4Only)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", IpVersionPreference.Ipv4Only)]
    public void EnrichLogWithClientIp_WithIpv4OnlyPreferenceAndIpv6Address_ShouldNotLogIp(string ip,
        IpVersionPreference preference)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, preference);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }

    [Theory]
    [InlineData("192.168.1.1", IpVersionPreference.Ipv6Only)]
    [InlineData("127.0.0.1", IpVersionPreference.Ipv6Only)]
    public void EnrichLogWithClientIp_WithIpv6OnlyPreferenceAndIpv4Address_ShouldNotLogIp(string ip,
        IpVersionPreference preference)
    {
        // Arrange
        IPAddress ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = ipAddress;

        ClientIpEnricher ipEnricher = new(_contextAccessor, preference);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }

    [Fact]
    public void WithClientIp_WithIpVersionPreference_ShouldNotThrowException()
    {
        // Arrange
        Logger logger = new LoggerConfiguration()
            .Enrich.WithClientIp(IpVersionPreference.Ipv4Only)
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act
        Exception exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ClientIpEnricher_WithDefaultConstructor_ShouldUseNonePreference()
    {
        // Arrange
        ClientIpEnricher ipEnricher = new(_contextAccessor);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");

        // Assert - Should log the IP since default is None preference
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("192.168.1.1", evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void ClientIpEnricher_WithPreferenceConstructor_ShouldUseSpecifiedPreference()
    {
        // Arrange
        ClientIpEnricher ipEnricher = new(_contextAccessor, IpVersionPreference.Ipv4Only);
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("::1");

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information("Has an IP property");

        // Assert - Should not log IPv6 with IPv4Only preference
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }
}