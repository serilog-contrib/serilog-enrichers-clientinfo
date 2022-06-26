namespace Serilog.Enrichers
{
    internal class ClinetIpConfiguration
    {
        public static string XForwardHeaderName { get; set; } = "X-forwarded-for";
    }
}