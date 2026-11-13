using System;
using System.Reflection;

namespace AutoCall.Common.Attributes {
    public class DisplayAttribute : Attribute {

        public bool Display { get; }
        public int Order { get; }

        public DisplayAttribute(bool display, int order = 0) {
            this.Display = display;
            this.Order = order;
        }

        public static int GetOrder(Enum @enum) {
            Type type = @enum.GetType();

            MemberInfo[] memberInfo = type.GetMember(@enum.ToString());
            if (memberInfo.Length > 0) {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes.Length > 0) {
                    return ((DisplayAttribute)attributes[0]).Order;
                }
            }
            return -1;
        }

        public static bool HasDisplay(Enum @enum) {
            Type type = @enum.GetType();

            MemberInfo[] memberInfo = type.GetMember(@enum.ToString());
            if (memberInfo.Length > 0) {
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes.Length > 0) {
                    return ((DisplayAttribute)attributes[0]).Display;
                }
            }
            return false;
        }
    }
}
