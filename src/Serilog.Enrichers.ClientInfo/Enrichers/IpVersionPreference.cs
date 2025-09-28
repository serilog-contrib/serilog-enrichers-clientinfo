namespace Serilog.Enrichers;

/// <summary>
///     Specifies the IP version preference for client IP enrichment.
/// </summary>
public enum IpVersionPreference
{
    /// <summary>
    ///     No preference - use whatever IP version is available (default behavior).
    /// </summary>
    None = 0,

    /// <summary>
    ///     Prefer IPv4 addresses when available, fallback to IPv6 if IPv4 is not available.
    /// </summary>
    PreferIpv4 = 1,

    /// <summary>
    ///     Prefer IPv6 addresses when available, fallback to IPv4 if IPv6 is not available.
    /// </summary>
    PreferIpv6 = 2,

    /// <summary>
    ///     Only log IPv4 addresses, ignore IPv6 addresses.
    /// </summary>
    Ipv4Only = 3,

    /// <summary>
    ///     Only log IPv6 addresses, ignore IPv4 addresses.
    /// </summary>
    Ipv6Only = 4
}