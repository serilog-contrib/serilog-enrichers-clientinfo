using Microsoft.AspNetCore.Http;
using NSubstitute;
using Serilog.Events;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests
{
    public class ClientAgentEnricherTests
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public ClientAgentEnricherTests()
        {
            var httpContext = new DefaultHttpContext();
            _contextAccessor = Substitute.For<IHttpContextAccessor>();
            _contextAccessor.HttpContext.Returns(httpContext);
        }

        [Fact]
        public void When_Enrich_Log_Event_ClientAgentEnricher_Should_Contain_ClientAgent_Property()
        {
            // Arrange
            var agentEnricher = new ClientAgentEnricher(_contextAccessor);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .Enrich.With(agentEnricher)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            // Act
            log.Information(@"Has an Agent property");

            // Assert
            Assert.NotNull(evt);
            Assert.True(evt.Properties.ContainsKey("ClientAgent"));
            Assert.True(string.IsNullOrEmpty(evt.Properties["ClientAgent"].LiteralValue().ToString()));
        }

        [Fact]
        public void When_Enrich_Log_Event_ClientAgentEnricher_And_Request_Contain_UserAgentHeader_Should_ClientAgentProperty_Have_Value()
        {
            // Arrange
            _contextAccessor.HttpContext.Request.Headers.Add("User-Agent", "Test Agent");

            var agentEnricher = new ClientAgentEnricher(_contextAccessor);

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .Enrich.With(agentEnricher)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            // Act
            log.Information(@"Has an Agent property");

            // Assert
            Assert.NotNull(evt);
            Assert.True(evt.Properties.ContainsKey("ClientAgent"));
            Assert.Equal("Test Agent", evt.Properties["ClientAgent"].LiteralValue().ToString());
        }
    }
}