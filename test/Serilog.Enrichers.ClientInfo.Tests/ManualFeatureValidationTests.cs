using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ManualFeatureValidationTests
{
    [Fact]
    public void DemonstrateIpv4OnlyFeature()
    {
        // Arrange
        DefaultHttpContext httpContext = new();
        IHttpContextAccessor contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(httpContext);

        // Test with IPv4 address
        contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        ClientIpEnricher ipv4OnlyEnricher = new(contextAccessor, IpVersionPreference.Ipv4Only);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipv4OnlyEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information(@"Test IPv4 logging");

        // Assert IPv4 address is logged
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("192.168.1.1", evt.Properties["ClientIp"].LiteralValue());

        // Test with IPv6 address - should not log
        evt = null;
        contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("::1");

        log.Information(@"Test IPv6 with IPv4 only");

        // Assert IPv6 address is NOT logged
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }

    [Fact]
    public void DemonstrateIpv6OnlyFeature()
    {
        // Arrange
        DefaultHttpContext httpContext = new();
        IHttpContextAccessor contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(httpContext);

        // Test with IPv6 address
        contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("::1");
        ClientIpEnricher ipv6OnlyEnricher = new(contextAccessor, IpVersionPreference.Ipv6Only);

        LogEvent evt = null;
        Logger log = new LoggerConfiguration()
            .Enrich.With(ipv6OnlyEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information(@"Test IPv6 logging");

        // Assert IPv6 address is logged
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("::1", evt.Properties["ClientIp"].LiteralValue());

        // Test with IPv4 address - should not log
        evt = null;
        contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        log.Information(@"Test IPv4 with IPv6 only");

        // Assert IPv4 address is NOT logged
        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }

    [Fact]
    public void DemonstrateExtensionMethodUsage()
    {
        // Test that the new extension method works correctly
        Logger logger1 = new LoggerConfiguration()
            .Enrich.WithClientIp() // Default - no preference
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        Logger logger2 = new LoggerConfiguration()
            .Enrich.WithClientIp(IpVersionPreference.Ipv4Only) // IPv4 only
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        Logger logger3 = new LoggerConfiguration()
            .Enrich.WithClientIp(IpVersionPreference.Ipv6Only) // IPv6 only
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();

        // Act - just make sure no exceptions are thrown
        Exception exception1 = Record.Exception(() => logger1.Information("Test"));
        Exception exception2 = Record.Exception(() => logger2.Information("Test"));
        Exception exception3 = Record.Exception(() => logger3.Information("Test"));

        // Assert
        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
    }
}