using AutoCall.Common.Extensions;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Helpers {
    public static class LocationHelper {
        public static bool CheckLocation(ref string location) {
            location = location.Trim().ReplaceAll("  ", " ");

            if (location.Count(ch => ch == ',') != 1) {
                return false;
            }

            if (Regex.IsMatch(location, @"^[a-zA-Z\s\.'`]+\,\s[A-Za-z]{2}(\s[\d\s\w]+)?$")) {
                var arr = location.Split(',');
                location = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(arr[0].Trim()) + ", " + arr[1].Trim().ToUpper();
                return true;
            } else {
                return false;
            }
        }
    }
}
