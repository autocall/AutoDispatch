using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoDispatch.Web.Common.Extensions;

/// <summary>
///     Строковые хелперы dispatch из StringExtension и DispatchHelper AutoCall.Common V1 </summary>
public static class DispatchStringExtension {

    public static string OnlyNumbers(this string str) {
        if (str == null) return null;
        return new String(str.Where(Char.IsDigit).ToArray());
    }

    public static string OnlyLetters(this string str) {
        if (str == null) return null;
        return new String(str.Where(Char.IsLetter).ToArray());
    }

    public static string OnlyNumbersAndLetters(this string str) {
        if (str == null) return null;
        return new String(str.Where(e => Char.IsDigit(e) || Char.IsLetter(e)).ToArray());
    }

    public static string OnlyDecimal(this string str) {
        if (str == null) return null;
        return new String(str.Where(e => Char.IsDigit(e) || e == '.' || e == ',').ToArray());
    }

    /// <summary>
    ///     (520) 223-0230 -> 15202230230 </summary>
    public static string ToUsPhone(string phone) {
        if (phone == null) return null;
        phone = OnlyNumbers(phone);
        return phone.Length == 10 ? "1" + phone : phone;
    }

    /// <summary>
    ///     (520) 223-0230 -> 5202230230 </summary>
    public static string ToLocalPhone(string phone) {
        if (phone == null) return null;
        phone = OnlyNumbers(phone);
        if (phone.Length == 11) {
            return phone.Substring(1, 10);
        }
        return phone;
    }

    /// <summary>
    ///     5202230230 -> (520) 223-0230, 11 цифр -> +1 (520) 223-0230 </summary>
    public static string ToPhone(this string phone) {
        phone = phone.OnlyNumbers();
        if (phone.Length == 10) {
            return new StringBuilder(14).Append('(').Append(phone, 0, 3).Append(") ").Append(phone, 3, 3).Append('-').Append(phone, 6, 4).ToString();
        }
        if (phone.Length == 11) {
            return new StringBuilder(14).Append("+").Append(phone, 0, 1).Append(" (").Append(phone, 1, 3).Append(") ")
                .Append(phone, 4, 3).Append('-').Append(phone, 7, 4).ToString();
        }
        return phone;
    }

    public static string ReplaceAll(this string str, string oldValue, string newValue) {
        var result = str.Replace(oldValue, newValue);
        return result != str ? ReplaceAll(result, oldValue, newValue) : result;
    }

    public static string ToFileName(string name) {
        if (name == null) return null;
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        return Regex.Replace(name, invalidRegStr, "_");
    }

    public static string NullableToString<T>(this T? value) where T : struct {
        return value.HasValue ? value.ToString() : "";
    }

    public static string NullableToString(this decimal? value, string format) {
        return value.HasValue ? value.Value.ToString(format, CultureInfo.InvariantCulture) : "";
    }

    public static string NullableLengthToString(this int? value) {
        return value.HasValue ? $"{value} ft" : "";
    }

    #region [ DispatchHelper V1: даты встреч в письмах ]

    public static string DateTimeToAppointmentFullDateTime(DateTime date, TimeSpan time1, TimeSpan time2) {
        if (time1 == time2) {
            return String.Format("{0:M/d/yyyy} App {1:h:mmtt}", date, date.Date + time1);
        }
        return String.Format("{0:M/d/yyyy} {1:h:mmtt} - {2:h:mmtt}", date, date.Date + time1, date.Date + time2);
    }

    #endregion [ DispatchHelper V1: даты встреч в письмах ]
}
