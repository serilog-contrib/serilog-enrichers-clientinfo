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

        [Fact]
        public void When_Enrich_Log_Event_With_IpEnricher_Should_Contain_ClientIp_Property()
        {
            // Arrange
            _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("::1");

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
            Assert.Equal("::1", evt.Properties["ClientIp"].LiteralValue());
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

        [Fact]
        public void When_Enrich_Log_Event_With_IpEnricher_AndRequest_Contain_ForwardHeader_Should_Read_ClientIp_Value_From_Header_Value()
        {
            //Arrange
            _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
            _contextAccessor.HttpContext.Request.Headers.Add("X-forwarded-for", IPAddress.Broadcast.ToString());

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
            Assert.Equal(IPAddress.Broadcast.ToString(), evt.Properties["ClientIp"].LiteralValue());
        }

        [Fact]
        public void When_Enrich_Log_Event_ClientAgentEnricher_Should_Contain_ClientAgent_Property()
        {
            var httpContext = new DefaultHttpContext();
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(httpContext);

            var agentEnricher = new ClientAgentEnricher(contextAccessor);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .Enrich.With(agentEnricher)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information(@"Has an Agent property");

            Assert.NotNull(evt);
            Assert.True(evt.Properties.ContainsKey("ClientAgent"));
            Assert.True(string.IsNullOrEmpty(evt.Properties["ClientAgent"].LiteralValue().ToString()));
        }

        [Fact]
        public void When_Enrich_Log_Event_ClientAgentEnricher_And_Request_Contain_UserAgentHeader_Should_ClientAgentProperty_Have_Value()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("User-Agent", "Test Agent");
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(httpContext);

            var agentEnricher = new ClientAgentEnricher(contextAccessor);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .Enrich.With(agentEnricher)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information(@"Has an Agent property");

            Assert.NotNull(evt);
            Assert.True(evt.Properties.ContainsKey("ClientAgent"));
            Assert.Equal("Test Agent", evt.Properties["ClientAgent"].LiteralValue().ToString());
        }
    }
}