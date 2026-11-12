namespace AutoDispatch.Web.Common.Extensions;

public static class StringExtension {

    public static string Truncate(this string value, int maxLength) {
        if (String.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    public static string TruncateToken(this string value) {
        if (String.IsNullOrEmpty(value)) return value;
        var maxLength = 20;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }

    /// <summary>
    ///     Короткая запись длительности для логов: 0s, 0.4s, 12s, 3m 5s, 1h 2m </summary>
    public static string ToString(TimeSpan ts) {
        if (ts < TimeSpan.Zero) {
            return $"-{ToString(-ts)}";
        }
        if (ts < TimeSpan.FromSeconds(0.1)) {
            return "0s";
        }
        if (ts < TimeSpan.FromSeconds(1)) {
            return $"{ts.TotalSeconds:F1}s";
        }
        if (ts < TimeSpan.FromMinutes(1)) {
            return $"{(int)ts.TotalSeconds}s";
        }
        if (ts < TimeSpan.FromHours(1)) {
            return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
        }
        return $"{(int)ts.TotalHours}h {ts.Minutes}m";
    }
}
