using System.Globalization;
using System.Text.RegularExpressions;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Проверка и нормализация "City, ST" (V1 AutoDispatch.Logic.Helpers.LocationHelper, контракты App) </summary>
public static class LocationHelper {

    public static bool CheckLocation(ref string location) {
        location = location.Trim().ReplaceAll("  ", " ");
        if (location.Count(ch => ch == ',') != 1) {
            return false;
        }
        if (Regex.IsMatch(location, @"^[a-zA-Z\s\.'`]+\,\s[A-Za-z]{2}(\s[\d\s\w]+)?$")) {
            var arr = location.Split(',');
            location = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(arr[0].Trim()) + ", " + arr[1].Trim().ToUpper();
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    ///     Код штата из "City, ST ..." либо null </summary>
    public static string GetStateCode(string location) {
        if (String.IsNullOrEmpty(location)) return null;
        var match = Regex.Match(location, DispatchHelper.LocationRegex);
        return match.Success ? match.Groups["state"].Value.ToUpper() : null;
    }
}
