using System;

namespace AutoCall.Common.Helpers {
    public class DateTimeHelper {
        public static DateTime FromUnixTime(long unixTime) {
            return epoch.AddMilliseconds(unixTime);
        }
        public static long ToUnixTime(DateTime ticks) {
            return (long)(ticks - epoch).TotalMilliseconds;
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
