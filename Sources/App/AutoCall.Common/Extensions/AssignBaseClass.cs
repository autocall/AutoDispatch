using System.Reflection;

namespace AutoCall.Common.Extensions {
    /// <summary>
    ///     Class for assigning properties </summary>
    /// TODO: Not using
    public static class AssignBaseClass {
        /// <summary>
        ///     Assign properties from <paramref name="a"/> to <paramref name="b"/> </summary>
        /// <typeparam name="T">Source Type</typeparam>
        /// <typeparam name="F">Destination Type</typeparam>
        /// <param name="a">Source</param>
        /// <param name="b">Destination</param>
        public static void AssignBaseProperties<T, F>(T a, F b) where T : F {
            foreach (PropertyInfo propertyInfo in b.GetType().GetProperties()) {
                object value = propertyInfo.GetValue(b, null);
                propertyInfo.SetValue(a, value, null);
            }
        }
    }
}
