using System;

namespace AutoCall.Common.Helpers {
    public static class MathHelper {
        /// <summary>
        ///     Converts DateTime to 62 Number System (1-0, a-z, A-Z) minus 2019 years </summary>
        /// <remarks>https://stackoverflow.com/questions/923771/quickest-way-to-convert-a-base-10-number-to-any-base-in-net</remarks>
        public static string DateTimeNowToScale62() {
            return IntToString(long.Parse(DateTime.Now.ToString("MMddHHmm")),
                new char[] { '0','1','2','3','4','5','6','7','8','9',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'});
        }

        public static string IntToString(long value, char[] baseChars) {
            string result = string.Empty;
            long targetBase = baseChars.Length;

            do {
                result = baseChars[value % targetBase] + result;
                value = value / targetBase;
            }
            while (value > 0);

            return result;
        }
        public static DateTime Min(DateTime val1, DateTime val2) {
            if (val1 > val2) {
                return val2;
            } else {
                return val1;
            }
        }
        public static DateTime Max(DateTime val1, DateTime val2) {
            if (val1 < val2) {
                return val2;
            } else {
                return val1;
            }
        }

        private const float LOCATION_EPSILON = 0.01f;
        public static bool LocationCompare(float a, float b) {
            return Math.Abs(a - b) < LOCATION_EPSILON;
        }
    }
}
