using System;

namespace AutoCall.Web.Common.Helpers {
    public static class DistanceHelper {
        /// <summary>
        ///     Calculate distance between two points </summary>
        /// <remarks>http://stackoverflow.com/questions/5780360/converting-longitude-latitude-degrees-to-miles-in-java</remarks>
        /// <param name="lat1">Latitude of origin</param>
        /// <param name="long1">Longitude of origin</param>
        /// <param name="lat2">Latitude of destination</param>
        /// <param name="long2">Longitude of destination</param>
        /// <returns>Miles</returns>
        public static double GetDistance(double lat1, double long1, double lat2, double long2) {
            var x = 69.1 * (lat2 - lat1);
            var y = 69.1 * (long2 - long1) * Math.Cos(lat1 / 57.3);
            return Math.Sqrt(x * x + y * y);
        }

        public static float KilometersToMiles(float miles) {
            return miles / 1.60934f;
        }
    }
}
