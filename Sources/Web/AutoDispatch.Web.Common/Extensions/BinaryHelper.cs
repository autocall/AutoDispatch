using System.Text;

namespace AutoDispatch.Web.Common.Extensions;

/// <summary>
///     Байты ↔ строка UTF-16 (как BinaryHelper V1: формат снимков истории), hex, склейка, int-массивы NextLoadExtra </summary>
public static class BinaryHelper {

    public static byte[] GetBytes(string str) {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static string GetString(byte[] bytes) {
        char[] chars = new char[bytes.Length / sizeof(char)];
        Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    public static string ToHex(this byte[] bytes, bool upperCase = false) {
        var result = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) {
            result.Append(b.ToString(upperCase ? "X2" : "x2"));
        }
        return result.ToString();
    }

    public static byte[] Combine(params byte[][] arrays) {
        byte[] rv = new byte[arrays.Sum(a => a.Length)];
        int offset = 0;
        foreach (byte[] array in arrays) {
            Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
        }
        return rv;
    }

    /// <summary>
    ///     Converts Bytes to Ints </summary>
    public static int[] BytesToInts(this byte[] bytes, int destinationLength) {
        bytes = bytes ?? new byte[0];
        Array.Resize(ref bytes, destinationLength * 4);
        int[] ints = new int[bytes.Length / 4];
        for (var i = 0; i < bytes.Length; i += 4) {
            ints[i / 4] = BitConverter.ToInt32(bytes, i);
        }
        return ints;
    }

    /// <summary>
    ///     Converts Ints to Bytes </summary>
    public static byte[] IntsToBytes(this int[] ints, int sourceLength) {
        Array.Resize(ref ints, sourceLength);
        byte[][] bytes = new byte[ints.Length][];
        for (var i = 0; i < ints.Length; i++) {
            bytes[i] = BitConverter.GetBytes(ints[i]);
        }
        return Combine(bytes);
    }

    public static async Task WriteAllBytesAsync(string path, byte[] data) {
        if (File.Exists(path)) {
            File.Delete(path);
        }
        await File.WriteAllBytesAsync(path, data);
    }
}
