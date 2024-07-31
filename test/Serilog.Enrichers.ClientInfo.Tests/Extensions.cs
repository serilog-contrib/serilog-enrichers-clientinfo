using Serilog.Events;

namespace Serilog.Enrichers.ClientInfo.Tests;

internal static class Extensions
{
    public static object LiteralValue(this LogEventPropertyValue @this) => ((ScalarValue)@this).Value;
}