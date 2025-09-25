using Serilog.Events;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetRoot_ReturnsHelloWorld()
    {
        // Arrange
        const string ip = "1.2.3.4";

        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-forwarded-for", ip);

        // Act
        HttpResponseMessage response = await client.GetAsync("/");
        IReadOnlyList<LogEvent> logs = DelegatingSink.Logs;

        List<KeyValuePair<string, LogEventPropertyValue>> allClientIpLogs = logs
            .SelectMany(l => l.Properties)
            .Where(p => p.Key == "ClientIp")
            .ToList();

        List<KeyValuePair<string, LogEventPropertyValue>> forwardedClientIpLogs = logs
            .SelectMany(l => l.Properties)
            .Where(p => p.Key == "ClientIp" && p.Value.LiteralValue().Equals(ip))
            .ToList();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(allClientIpLogs.Count, forwardedClientIpLogs.Count);
    }
}