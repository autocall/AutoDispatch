using System;
using System.Collections.Generic;

namespace AutoCall.Common.Helpers {
    /// <summary>
    ///     Ограничивает выполнение по времени. выполнять дествие не чаще чем dt </summary>
    public static class LimitExecutionHelper {

        private static Dictionary<string, DateTime> Times = new Dictionary<string, DateTime>();

        public static bool Check(string key, TimeSpan dt) {
            if (Times.ContainsKey(key) == false) {
                Times[key] = DateTime.UtcNow;
                return true;
            } else {
                if (DateTime.UtcNow - Times[key] > dt) {
                    Times[key] = DateTime.UtcNow;
                    return true;
                } else {
                    return false;
                }
            }
        }

        public static bool Check(string key, TimeSpan firstDt, TimeSpan repeatDt) {
            if (Times.ContainsKey(key) == false) {
                Times[key] = DateTime.UtcNow - repeatDt + firstDt;
                return false;
            } 
            if (DateTime.UtcNow - Times[key] > repeatDt) {
                Times[key] = DateTime.UtcNow;
                return true;
            } else {
                return false;
            }
        }

        public static void Remove(string key) {
            if (Times.ContainsKey(key) == true) {
                Times.Remove(key);
            }
        }
    }
}
