using System;
using System.Collections.Generic;

namespace AutoCall.Common.Helpers {
    public static class StaticHelper {

        private static Dictionary<Type, object> _instatnces = new Dictionary<Type, object>();

        public static T GetInstatice<T>() {
            if (_instatnces.ContainsKey(typeof(T)) == false) {
                _instatnces[typeof(T)] = Activator.CreateInstance<T>();
            }
            return (T)_instatnces[typeof(T)];
        }
    }
}
