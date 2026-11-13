namespace AutoDispatch.Web.Common.Extensions;

public static class CollectionExtension {

    /// <summary>
    ///     Порциями по limit (V1 DivideBy) </summary>
    public static IEnumerable<List<T>> DivideBy<T>(this List<T> list, int limit = 1000) {
        if (list.Count > 0) {
            if (list.Count <= limit) {
                yield return list;
            } else {
                for (int i = 0; i < list.Count; i += limit) {
                    yield return list.Skip(i).Take(limit).ToList();
                }
            }
        }
    }
}
