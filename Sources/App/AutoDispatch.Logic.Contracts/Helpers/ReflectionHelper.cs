using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Helpers {
    public static class ReflectionHelper {
        public static Object GetPropValue(this Object obj, String propName) {
            string[] nameParts = propName.Split('.');
            if (nameParts.Length == 1) {
                return obj.GetType().GetProperty(propName).GetValue(obj, null);
            }

            foreach (String part in nameParts) {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                var match = new Regex(@"^(?<prop>[\d\w]+)\[(?<index>\d+)\]$").Match(part);

                if (match.Success) {
                    var index = Int32.Parse(match.Groups["index"].Value);
                    var prop = match.Groups["prop"].Value;

                    PropertyInfo info = type.GetProperty(prop);
                    if (info == null) { return null; }

                    obj = info.GetValue(obj, null);
                    if (obj is IList list) {
                        obj = list[index];
                    } else {
                        throw new Exception();
                    }
                } else {
                    PropertyInfo info = type.GetProperty(part);
                    if (info == null) { return null; }

                    obj = info.GetValue(obj, null);
                }
            }
            return obj;
        }

        public static PropertyInfo GetProp(this Type type, String propName) {
            string[] nameParts = propName.Split('.');
            if (nameParts.Length == 1) {
                return type.GetProperty(propName);
            }

            PropertyInfo prop = null;
            foreach (String part in nameParts) {
                prop = type.GetProperty(part);
                if (prop == null) return null;
                type = prop.PropertyType;
            }
            return prop;
        }
    }
}
