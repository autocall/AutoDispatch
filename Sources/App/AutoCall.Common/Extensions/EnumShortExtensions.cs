using System;
using System.Reflection;
using AutoCall.Common.Attributes;

namespace AutoCall.Common.Extensions {
    /// <summary>
    ///     GetDescriptionShort из EnumExtensions V1: остальное EnumExtensions — в общем контракте AutoDispatch.Dtos (Sources/Shared,
    ///     тот же namespace), где DescriptionShortAttribute нет </summary>
    public static class EnumShortExtensions {

        /// <summary>
        ///     Gets description from attribute </summary>
        /// <param name="element">Enum element</param>
        /// <returns>Description text</returns>
        public static string GetDescriptionShort(this Enum element) {
            Type type = element.GetType();

            MemberInfo[] memberInfo = type.GetMember(element.ToString());
            if (memberInfo.Length > 0) {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionShortAttribute), false);
                if (attributes.Length > 0) {
                    return ((DescriptionShortAttribute)attributes[0]).Description;
                }
            }
            return null;
        }
    }
}
