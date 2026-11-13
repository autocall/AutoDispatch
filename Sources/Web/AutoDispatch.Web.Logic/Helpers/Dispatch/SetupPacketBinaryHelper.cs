namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Битовые поля setup packet (Step8Modes, Step8Commodities — bool; NumberOfUnits, W8States — short) ↔ byte[] (V1) </summary>
public static class SetupPacketBinaryHelper {

    public static T[] Convert<T>(byte[] binary, int? length = null) {
        T[] arr = null;

        if (typeof(T) == typeof(bool)) {
            arr = ConverToBools(binary).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(short)) {
            arr = ConverToShorts(binary).Cast<T>().ToArray();
        }

        if (arr != null) {
            if (length.HasValue) {
                Array.Resize(ref arr, length.Value);
            }
            return arr;
        } else {
            throw new Exception();
        }
    }

    public static byte[] BackConvert<T>(List<T> data) {
        byte[] arr = null;

        if (typeof(T) == typeof(bool)) {
            arr = ConvertFromBools(data.Cast<bool>().ToArray()).ToArray();
        }
        if (typeof(T) == typeof(short)) {
            arr = ConvertFromShorts(data.Cast<short>()).ToArray();
        }

        if (arr != null) {
            return arr;
        } else {
            throw new Exception();
        }
    }

    private static IEnumerable<bool> ConverToBools(byte[] binary) {
        return binary.SelectMany(GetBooleans);
    }

    private static IEnumerable<short> ConverToShorts(byte[] binary) {
        Array.Resize(ref binary, binary.Length + (2 - binary.Length % 2) % 2); // sets len % 2
        for (int i = 0; i < binary.Length; i += 2) {
            yield return BitConverter.ToInt16(binary, i);
        }
    }

    private static IEnumerable<byte> ConvertFromBools(bool[] data) {
        Array.Resize(ref data, data.Length + (8 - data.Length % 8) % 8); // sets len % 8
        for (int i = 0; i < data.Length; i += 8) {
            yield return GetBooleans(data.Skip(i).Take(8));
        }
    }

    private static IEnumerable<byte> ConvertFromShorts(IEnumerable<short> data) {
        return data.SelectMany(BitConverter.GetBytes);
    }

    private static IEnumerable<bool> GetBooleans(byte b) {
        for (int i = 0; i < 8; i++) {
            yield return (b & (1 << i)) != 0;
        }
    }

    private static byte GetBooleans(IEnumerable<bool> arr) {
        byte i = 0;
        byte val = 0;
        foreach (var b in arr) {
            if (b) {
                val += (byte)(1 << i);
            }
            i++;
        }
        return val;
    }
}
