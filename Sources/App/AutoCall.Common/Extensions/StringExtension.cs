using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoCall.Common.Extensions {
    public static class StringExtension {
        public static int GetDeterministicHashCode(this string str) {
            if (String.IsNullOrEmpty(str)) return 0;
            unchecked {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        /// <summary>
        ///     Convert Object to list of string properties </summary>
        public static string ObjectToString(Object obj) {
            var result = String.Empty;
            foreach (var property in obj.GetType().GetProperties()) {
                if (property.PropertyType.GetInterface(typeof(IList).Name) != null) {
                    continue;
                }
                try {
                    var val = property.GetValue(obj);
                    var str = val?.ToString() ?? "null";
                    result += $"{property.Name}: {str.Truncate(36)}\n";
                } catch (Exception exception) {
                }
            }
            return result;
        }
        public static string NullableToString<T>(this T? value) where T : struct {
            if (value.HasValue) return value.ToString();
            return "";
        }
        public static string NullableToString(this decimal? value, string format = null) {
            if (value.HasValue) return value.Value.ToString(format);
            return "";
        }
        public static string NullableToString(this float? value, string format = null) {
            if (value.HasValue) return value.Value.ToString(format);
            return "";
        }
        public static string NullableJoin(string separator, IEnumerable<string> arr) {
            arr = arr.Where(e => e != null);
            if (arr.Any()) {
                if (arr.Count() == 1 && arr.First() == null) {
                    return null;
                } else {
                    return String.Join(separator, arr);
                }
            } else {
                return null;
            }
        }
        /// <summary>
        ///     Concatinate objects as strings </summary>
        public static string ToAggregateString<T>(this IList<T> patterns) {
            return patterns.Select(p => (p?.ToString() ?? "null") + "\n").Aggregate("", (p1, p2) => p1 + p2);
        }
        /// <summary>
        ///     Concatinate objects as strings </summary>
        public static string ToAggregateString<T>(this List<T> patterns) {
            return ToAggregateString(patterns as IList<T>);
        }
        /// <summary>
        ///     Concatinate objects as strings </summary>
        public static string ToAggregateString(this IList patterns) {
            return patterns.Cast<object>().Select(p => (p?.ToString() ?? "null") + "\n").Aggregate("", (p1, p2) => p1 + p2);
        }
        /// <summary>
        ///     Concatinate objects as strings </summary>
        public static string ToAggregateString<T>(this T[] patterns) {
            return patterns.Cast<object>().Select(p => (p?.ToString() ?? "null") + "\n").Aggregate("", (p1, p2) => p1 + p2);
        }

        public static string ToAppointment(DateTime dt1, DateTime dt2) {
            if (dt1.TimeOfDay == dt2.TimeOfDay) {
                return String.Format("{0:M/d} Appt {0:h:mmtt}", dt1);
            } else {
                return String.Format("{0:M/d} {0:h:mmtt} - {1:h:mmtt}", dt1, dt2);
            }
        }

        public static string Truncate(this string value, int maxLength) {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string ExtendLast(this string value, int lengthString, char ch) {
            if (String.IsNullOrEmpty(value)) return value;
            if (value.Length < lengthString) {
                return value + new String(ch, lengthString - value.Length);
            } else {
                return value;
            }
        }

        public static string ExtendFirst(this string value, int lengthString, char ch) {
            if (String.IsNullOrEmpty(value)) return value;
            if (value.Length < lengthString) {
                return new String(ch, lengthString - value.Length) + value;
            } else {
                return value;
            }
        }

        public static string FixSize(this string value, int length) {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= length ? value + new String(' ', length - value.Length) : value.Substring(0, length);
        }

        public static string ToSize(long value) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = Math.Abs(value);
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length) {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = $"{(value >= 0 ? len : -len):0.#}{sizes[order]}";
            return result;
        }

        public static string ToString(TimeSpan ts) {
            if (ts < TimeSpan.FromSeconds(0.1)) {
                return $"0s";
            }
            if (ts < TimeSpan.FromSeconds(1)) {
                return $"{ts.TotalSeconds:F1}s";
            }
            if (ts < TimeSpan.FromMinutes(1)) {
                return $"{(int)ts.TotalSeconds}s";
            }
            if (ts < TimeSpan.FromHours(1)) {
                return $"{(int)ts.TotalMinutes}m";
            }
            if (ts.TotalHours < 2) {
                return $"{(int)ts.TotalHours}h.{ts.Minutes:D2}m";
            }
            if (ts.TotalHours < 24) {
                return $"{(int)ts.TotalHours}h";
            }
            if (ts.TotalDays < 10) {
                return $"{ts.TotalDays:F1}d";
            } else {
                return $"{(int)ts.TotalDays}d";
            }
        }
        public static string ToString(TimeSpan? ts) {
            if (ts == null) return null;
            return ToString(ts.Value);
        }
        public static string ToString(DateTime dt) {
            return ToString(DateTime.UtcNow - dt);
        }
        public static string ToString(DateTime? dt) {
            if (dt == null) return null;
            return ToString(DateTime.UtcNow - dt.Value);
        }

        public static string ToTotalSuffix(long value, bool fraction = true) {
            string[] sizes = { "", "K", "M", "G" };
            double len = (long)value;
            int order = 0;
            while (len >= 1000 && order + 1 < sizes.Length) {
                order++;
                len = len / 1000;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            if (fraction) {
                return $"{len:0.#}{sizes[order]}";
            } else {
                return $"{len:0}{sizes[order]}";
            }
        }

        public static IEnumerable<string> Split(this string str, int chunkSize) {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static string FirstCharToUpper(this string input) {
            if (String.IsNullOrEmpty(input)) {
                return null;
            }
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }

        public static string[] SplitLast(string s, char ch) {
            int idx = s.LastIndexOf(ch);
            return new string[] {
                s.Substring(0, idx),
                s.Substring(idx + 1)
            };
        }
        public static string[] SplitLast(string s, char[] ch) {
            int idx = s.LastIndexOfAny(ch);
            return new string[] {
                s.Substring(0, idx),
                s.Substring(idx + 1)
            };
        }

        public static string[] SplitFirst(string s, char ch) {
            int idx = s.IndexOf(ch);
            return new string[] {
                s.Substring(0, idx),
                s.Substring(idx + 1)
            };
        }

        public static string NormalizeCityName(string city) {
            if (String.IsNullOrEmpty(city) || city.Contains(',') == false) {
                return city;
            } else {

                try {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    var arr = StringExtension.SplitLast(city.TrimEnd(" US", StringComparison.OrdinalIgnoreCase).ToLower(), ',');
                    return arr.Length == 0
                        ? city.ToUpper()
                        : $"{textInfo.ToTitleCase(arr.First().Trim())}, {arr.Last().ToUpper().Trim()}";
                } catch (Exception) {
                    return city;
                }
            }
        }
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType) {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType)) {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            } else return input;
        }

        public static string DivExtend(this string str, int value) {
            if (str.Length % value != 0) {
                return str + new string(' ', value - str.Length % value);
            } else {
                return str;
            }
        }

        public static string NormalizeJson(this string str, char ch = '}') {
            return str.Substring(0, str.LastIndexOf(ch) + 1);
        }

        public static string ToMask(this string input, string mask) {
            int j = 0;
            var result = new List<char>();

            for (var i = 0; i < mask.Length; i++) {
                if (mask[i] == '#') {
                    if (j < input.Length) {
                        result.Add(input[j]);
                        j++;
                        continue;
                    }
                }
                result.Add(mask[i]);
            }
            return new String(result.ToArray());
        }

        public static string ReplaceAll(this string str, String oldValue, String newValue) {
            var result = str.Replace(oldValue, newValue);
            if (result != str) {
                return ReplaceAll(result, oldValue, newValue);
            } else {
                return result;
            }
        }

        /// <summary>
        ///     (520) 223-0230 -> 15202230230 </summary>
        public static string ToUsPhone(string phone) {
            if (phone == null) return null;
            phone = OnlyNumbers(phone);
            if (phone.Length == 10) {
                return "1" + phone;
            } else {
                return phone;
            }
        }

        /// <summary>
        ///     (520) 223-0230 -> 5202230230 </summary>
        public static string ToLocalPhone(string phone) {
            if (phone == null) return null;
            phone = OnlyNumbers(phone);
            if (phone.Length == 10) {
                return phone;
            } else
            if (phone.Length == 11) {
                return phone.Substring(1, 10);
            } else {
                return phone;
            }
        }

        public static string SetDtmf(this string phone, string dtmf) {
            phone = phone.OnlyNumbers();
            return new StringBuilder()
                .Append('(')
                .Append(phone, 0, 3)
                .Append(") ")
                .Append(phone, 3, 3)
                .Append('-')
                .Append(phone, 6, 4)
                .Append("\n(")
                .Append(dtmf)
                .Append(")")
                .ToString();
        }
        public static string AddDtmf(this string phone, string dtmf) {
            phone = phone.OnlyNumbers();
            return new StringBuilder()
                .Append('(')
                .Append(phone, 0, 3)
                .Append(") ")
                .Append(phone, 3, 3)
                .Append('-')
                .Append(phone, 6, 4)
                .Append("\n(")
                .Append(phone, 10, phone.Length - 10)
                .Append(dtmf)
                .Append(")")
                .ToString();
        }

        public static string OnlyNumbers(this string str) {
            if (str == null) return null;
            return new String(str.Where(e => Char.IsDigit(e)).ToArray());
        }
        public static string OnlyLetters(this string str) {
            if (str == null) return null;
            return new String(str.Where(e => Char.IsLetter(e)).ToArray());
        }
        public static string OnlyNumbersAndLetters(this string str) {
            if (str == null) return null;
            return new String(str.Where(e => Char.IsDigit(e) || Char.IsLetter(e)).ToArray());
        }
        public static string OnlyFloats(this string str) {
            if (str == null) return null;
            return new String(str.Where(e => Char.IsDigit(e) || e == '.').ToArray());
        }
        public static string OnlyDecimal(this string str) {
            if (str == null) return null;
            return new String(str.Where(e => Char.IsDigit(e) || e == '.' || e == ',').ToArray());
        }

        public static string TrimEndTo(string str, string sub) {
            var index = str.LastIndexOf(sub);
            if (index != -1) {
                return str.Substring(0, index);
            } else {
                return str;
            }
        }

        public static string Reverse1(this string str) {
            return new String(str.Reverse().ToArray());
        }

        public static string ToFileName(string name) {
            if (name == null) return null;
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        public static bool IsValidEmail(string email) {
            return new EmailAddressAttribute().IsValid(email);
        }

        public static string ToPhone(this string phone) {
            phone = phone.OnlyNumbers();
            if (phone.Length == 10) {
                // "(###) ###-####";
                return new StringBuilder(14)
                      .Append('(')
                      .Append(phone, 0, 3)
                      .Append(") ")
                      .Append(phone, 3, 3)
                      .Append('-')
                      .Append(phone, 6, 4)
                      .ToString();
            }
            if (phone.Length == 11) {
                //"+# (###) ###-####";
                return new StringBuilder(14)
                    .Append("+")
                    .Append(phone, 0, 1)
                    .Append(" (")
                    .Append(phone, 1, 3)
                    .Append(") ")
                    .Append(phone, 4, 3)
                    .Append('-')
                    .Append(phone, 7, 4)
                    .ToString();
            }
            return phone;
        }

        public static string ToPhone2(this string phone) {
            Debug.WriteLine(phone);
            phone = phone.OnlyNumbers().TrimStart('1');
            if (phone.Length == 10) {
                // "(###) ###-####";
                return new StringBuilder()
                    .Append('(')
                    .Append(phone, 0, 3)
                    .Append(") ")
                    .Append(phone, 3, 3)
                    .Append('-')
                    .Append(phone, 6, 4)
                    .ToString();
            }
            if (phone.Length > 10) {

                // "(###) ###-#### (##)";
                var result = new StringBuilder()
                     .Append('(')
                     .Append(phone, 0, 3)
                     .Append(") ")
                     .Append(phone, 3, 3)
                     .Append('-')
                     .Append(phone, 6, 4)
                     .Append(" (")
                     .Append(phone, 10, phone.Length - 10)
                     .Append(')')
                     .ToString();
                Debug.WriteLine(result);
                return result;
            }
            return phone;
        }

        public static bool ContainsWithoutSpace(this string str1, string str2) {
            return str1.Replace(" ", "").Contains(str2.Replace(" ", ""));
        }

        public static string FromBase64(this string value) {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        // log4net (common.logging) erroneous exception
        public static string NormalizeString(this string value) {
            return new String(value.Where(e => Char.IsLetterOrDigit(e) || Char.IsWhiteSpace(e) || Char.IsPunctuation(e) || e == '\n').ToArray());
        }
    }
}
