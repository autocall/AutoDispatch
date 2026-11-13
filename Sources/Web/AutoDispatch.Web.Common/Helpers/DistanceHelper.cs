namespace AutoDispatch.Web.Common.Helpers;

public static class DistanceHelper {

    /// <summary>
    ///     Расстояние между точками в милях, плоское приближение (V1 DistanceHelper) </summary>
    public static double GetDistance(double lat1, double long1, double lat2, double long2) {
        var x = 69.1 * (lat2 - lat1);
        var y = 69.1 * (long2 - long1) * Math.Cos(lat1 / 57.3);
        return Math.Sqrt(x * x + y * y);
    }
}
