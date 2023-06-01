using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using System.Net;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests
{
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
        public void When_Enrich_Log_Event_With_IpEnricher_Should_Contain_ClientIp_Property(string ip)
        {
            // Arrange
            var ipAddress = IPAddress.Parse(ip);
            _contextAccessor.HttpContext.Connection.RemoteIpAddress = ipAddress;

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
        public void When_Enrich_Log_Event_With_IpEnricher_And_Log_More_Than_Once_Should_Read_ClientIp_Value_From_HttpContext_Items()
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

        [Theory]
        [InlineData("::1")]
        [InlineData("192.168.1.1")]
        [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
        public void When_Enrich_Log_Event_With_IpEnricher_AndRequest_Contain_ForwardHeader_Should_Read_ClientIp_Value_From_Header_Value(string ip)
        {
            //Arrange
            var ipAddress = IPAddress.Parse(ip);
            _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
            _contextAccessor.HttpContext.Request.Headers.Add(ClinetIpConfiguration.XForwardHeaderName, ipAddress.ToString());

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
        public void When_Enrich_Log_Event_With_IpEnricher_With_Custom_XForwardHeader_AndRequest_Contain_ForwardHeader_Should_Read_ClientIp_Value_From_Header_Value()
        {
            //Arrange
            const string customForwardHeader = "CustomForwardHeader";
            _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
            _contextAccessor.HttpContext.Request.Headers.Add(customForwardHeader, IPAddress.Broadcast.ToString());

            var ipEnricher = new ClientIpEnricher(_contextAccessor);
            ClinetIpConfiguration.XForwardHeaderName = customForwardHeader;

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
    }
}