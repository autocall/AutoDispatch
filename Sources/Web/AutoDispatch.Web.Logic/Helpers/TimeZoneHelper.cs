namespace AutoDispatch.Web.Logic.Helpers;

public static class TimeZoneHelper {

    public const string UtcId = "UTC";

    private static List<string> _ianaIds;

    /// <summary>
    ///     IANA-идентификаторы зон системы (на Windows — через конвертацию из Windows Id), отсортированные </summary>
    public static List<string> GetIanaIds() {
        if (_ianaIds == null) {
            var ids = new HashSet<string> { UtcId };
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones()) {
                if (TimeZoneInfo.TryConvertWindowsIdToIanaId(tz.Id, out var iana)) {
                    ids.Add(iana);
                } else {
                    ids.Add(tz.Id);
                }
            }
            _ianaIds = ids.OrderBy(e => e).ToList();
        }
        return _ianaIds;
    }
}
