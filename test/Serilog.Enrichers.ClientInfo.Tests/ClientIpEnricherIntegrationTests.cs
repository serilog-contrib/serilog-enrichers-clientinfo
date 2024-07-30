using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class ClientIpEnricherIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetRoot_ReturnsHelloWorld()
    {
        // Arrange
        const string ip = "1.2.3.4";

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-forwarded-for", ip);

        // Act
        var response = await client.GetAsync("/");
        var logs = DelegatingSink.Logs;
        var allClientIpLogs = logs
            .SelectMany(l => l.Properties)
            .Where(p => p.Key == "ClientIp")
            .ToList();
        var forwardedClientIpLogs = logs
            .SelectMany(l => l.Properties)
            .Where(p => p.Key == "ClientIp" && p.Value.LiteralValue().Equals(ip))
            .ToList();

        // Assert
        Assert.All(forwardedClientIpLogs, l => l.Value.LiteralValue().Equals(ip));
    }
}