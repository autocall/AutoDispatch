using System.Security.Cryptography;

namespace AutoDispatch.Web.Data.Helpers;

/// <summary>
///     Ключи long без IDENTITY (БД логов): случайная точка + возрастающая серия (101,102,…; затем новая точка) — меньше page splits,
///     ключ известен до вставки </summary>
public class DbRandomHelper {

    private static readonly object _lock = new object();
    private const int COUNT_VALUES64 = 10000;

    private static long LastInt64Value = default;

    private static int CountInt64Value = Int32.MaxValue;

    public static long NewInt64() {
        lock (_lock) {
            if (CountInt64Value > COUNT_VALUES64) {
                LastInt64Value = Math.Abs(BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8), 0) % (Int64.MaxValue - COUNT_VALUES64 - 10_000)) + 10_000;
                CountInt64Value = 0;
            }
            CountInt64Value++;
            LastInt64Value++;
            return LastInt64Value;
        }
    }

}
