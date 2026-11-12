using System;
using System.Buffers;

namespace Serilog;

/// <summary>
///     Bounds and cleans client-supplied header values before they are written to logs.
/// </summary>
internal static class HeaderValueSanitizer
{
    /// <summary>
    ///     Default maximum length of a logged request header value.
    /// </summary>
    public const int DefaultMaxHeaderLength = 1024;

    /// <summary>
    ///     Default maximum length of an accepted correlation id.
    /// </summary>
    public const int DefaultMaxCorrelationIdLength = 128;

    // C0 and C1 control characters plus the Unicode line and paragraph separators: anything
    // that can start a new line or hide content in a text sink.
    private static readonly SearchValues<char> UnsafeChars = SearchValues.Create(BuildUnsafeChars());

    /// <summary>
    ///     Returns the value limited to <paramref name="maxLength" /> characters with control characters
    ///     removed, or null when nothing loggable is left.
    /// </summary>
    public static string Sanitize(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        ReadOnlySpan<char> span = value.AsSpan();
        if (span.Length > maxLength)
        {
            span = span[..maxLength];

            // Do not cut a surrogate pair in half.
            if (char.IsHighSurrogate(span[^1])) span = span[..^1];
        }

        int firstUnsafe = span.IndexOfAny(UnsafeChars);
        if (firstUnsafe < 0)
        {
            if (span.Length == value.Length) return value;
            return span.IsWhiteSpace() ? null : span.ToString();
        }

        char[] buffer = ArrayPool<char>.Shared.Rent(span.Length);
        try
        {
            span[..firstUnsafe].CopyTo(buffer);
            int length = firstUnsafe;
            foreach (char c in span[(firstUnsafe + 1)..])
            {
                if (!UnsafeChars.Contains(c)) buffer[length++] = c;
            }

            ReadOnlySpan<char> cleaned = buffer.AsSpan(0, length);
            return cleaned.IsWhiteSpace() ? null : cleaned.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    ///     Returns true when the value is non-empty, no longer than <paramref name="maxLength" /> and free of
    ///     control characters.
    /// </summary>
    public static bool IsValid(string value, int maxLength)
    {
        return !string.IsNullOrWhiteSpace(value)
               && value.Length <= maxLength
               && !value.AsSpan().ContainsAny(UnsafeChars);
    }

    private static string BuildUnsafeChars()
    {
        // 0x00-0x1F and 0x7F-0x9F are the C0 and C1 control ranges; 0x2028 and 0x2029 are the Unicode line
        // and paragraph separators.
        Span<char> chars = stackalloc char[67];
        int index = 0;
        for (int c = 0x00; c <= 0x1F; c++) chars[index++] = (char)c;
        for (int c = 0x7F; c <= 0x9F; c++) chars[index++] = (char)c;
        chars[index++] = (char)0x2028;
        chars[index] = (char)0x2029;
        return chars.ToString();
    }
}
