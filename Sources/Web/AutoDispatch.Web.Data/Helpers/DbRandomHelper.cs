using System.Security.Cryptography;

namespace AutoDispatch.Web.Data.Helpers;

/// <summary>
///     Ключи без IDENTITY: случайная точка + возрастающая серия (101,102,…; затем новая точка) — меньше page splits,
///     ключ известен до вставки </summary>
public class DbRandomHelper {

    private static readonly object _lock = new object();
    private const int COUNT_VALUES64 = 10000;
    private const int COUNT_VALUES32 = 1000;

    private static long LastInt64Value = default;
    private static int LastInt32Value = default;

    private static int CountInt64Value = Int32.MaxValue;
    private static int CountInt32Value = Int32.MaxValue;

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

    public static int NewInt32() {
        lock (_lock) {
            if (CountInt32Value > COUNT_VALUES32) {
                // нижняя граница выше системных Id (1000, 1001)
                LastInt32Value = RandomNumberGenerator.GetInt32(10_000, Int32.MaxValue - COUNT_VALUES32 - 1);
                CountInt32Value = 0;
            }
            CountInt32Value++;
            LastInt32Value++;
            return LastInt32Value;
        }
    }
}
