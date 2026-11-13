using System.Text;

namespace AutoDispatch.Web.Common.Extensions;

/// <summary>
///     V1 AutoCall.Common StringBuilderExtension.IndexOf (маркеры в RTF-шаблонах ReportService) </summary>
public static class StringBuilderExtension {

    public static int IndexOf(this StringBuilder haystack, string needle) {
        if (haystack == null || needle == null) {
            throw new ArgumentNullException();
        }
        if (needle.Length == 0) {
            return 0;
        }
        int max = haystack.Length - needle.Length;
        for (int i = 0; i <= max; i++) {
            if (haystack[i] != needle[0]) continue;
            int j = 1;
            while (j < needle.Length && haystack[i + j] == needle[j]) j++;
            if (j == needle.Length) return i;
        }
        return -1;
    }
}
