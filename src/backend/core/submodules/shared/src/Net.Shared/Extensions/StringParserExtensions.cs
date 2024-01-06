using System;

namespace Net.Shared.Extensions;

public static class StringParserExtensions
{
    public static T Parse<T>(this string value, IFormatProvider? formatProvider = null) where T : struct, ISpanParsable<T>
    {
        return T.Parse(value.AsSpan(), formatProvider);
    }

    public static bool TryParse<T>(this string value, out T result, IFormatProvider? formatProvider = null) where T : struct, ISpanParsable<T>
    {
        return T.TryParse(value.AsSpan(), formatProvider, out result);
    }
}
