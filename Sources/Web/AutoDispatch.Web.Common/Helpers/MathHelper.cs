namespace AutoDispatch.Web.Common.Helpers;

public static class MathHelper {

    private const float LOCATION_EPSILON = 0.01f;

    /// <summary>
    ///     Координаты равны с точностью 0.01 градуса (V1 MathHelper) </summary>
    public static bool LocationCompare(float a, float b) {
        return Math.Abs(a - b) < LOCATION_EPSILON;
    }
}
