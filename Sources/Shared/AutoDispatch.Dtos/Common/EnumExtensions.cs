using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AutoCall.Common.Extensions {
    /// <summary>
    ///     Копия AutoCall.Common.Extensions.EnumExtensions V1 без GetDescriptionShort (DescriptionShortAttribute контракту не нужен) </summary>
    public static class EnumExtensions {

        public static string GetDescription(this Enum element) {
            if (element == null) return null;
            Type type = element.GetType();

            MemberInfo[] memberInfo = type.GetMember(element.ToString());
            if (memberInfo.Length > 0) {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0) {
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }
            return null;
        }

        public static string GetDescriptions(this Enum element, string separator) {
            if (element.GetHashCode() == 0) return null;
            var values = element.ToString().Split(',').Select(s => s.Trim());
            var type = element.GetType();

            return String.Join(separator, values.Select(enumValue => type.GetMember(enumValue)
               .FirstOrDefault()
               ?.GetCustomAttribute<DescriptionAttribute>()
               ?.Description
               ?? enumValue.ToString()));
        }

        public static string GetDescriptionOrTostring(this Enum element) {
            var result = element.GetDescription();
            return result ?? element.ToString();
        }

        public static Enum ParseByDesctiption<TEnum>(string description) {
            return typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(fi => (Enum)fi.GetValue(null))
                .First(v => v.GetDescription() == description);
        }

        public static bool TryParseByDesctiption<TEnum>(string description, out TEnum enum1) where TEnum : struct {
            Enum result = typeof(TEnum).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(fi => (Enum)fi.GetValue(null))
                .FirstOrDefault(v => v.GetDescription() == description);
            if (result != null) {
                enum1 = (TEnum)Enum.ToObject(typeof(TEnum), result);
                return true;
            } else {
                enum1 = new TEnum();
                return false;
            }
        }

        public static Dictionary<int, string> EnumToDictionary<T>() {
            return typeof(T).GetEnumValues().Cast<Enum>().ToDictionary(Convert.ToInt32, GetDescription);
        }
    }
}
