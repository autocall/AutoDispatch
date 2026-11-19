using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Common.Helpers {
    /// <summary>
    ///     Represent class for Object to Binary and reverse </summary>
    public static class BinaryHelper {

        public static async Task WriteAllBytesAsync(string path, byte[] data) {
            if (File.Exists(path)) {
                File.Delete(path);
            }
            using (FileStream sourceStream = new FileStream(path,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true)) {
                await sourceStream.WriteAsync(data, 0, data.Length);
            };
        }

        // мёртвый код V1: ByteArrayToObject/ObjectToByteArray нигде не вызываются; BinaryFormatter в .NET 9+ удалён
//        /// <summary>
//        ///     Convert byte array to object </summary>
//        public static Object ByteArrayToObject(byte[] arrBytes) {
//            var memStream = new MemoryStream();
//            var binForm = new BinaryFormatter();
//            memStream.Write(arrBytes, 0, arrBytes.Length);
//            memStream.Seek(0, SeekOrigin.Begin);
//            var obj = binForm.Deserialize(memStream);
//            return obj;
//        }
//
//        /// <summary>
//        ///     Convert object to byte array </summary>
//        public static byte[] ObjectToByteArray(Object obj) {
//            if (obj == null)
//                return null;
//            var bf = new BinaryFormatter();
//            var ms = new MemoryStream();
//            bf.Serialize(ms, obj);
//            return ms.ToArray();
//        }

        /// <summary>
        ///     Copies the contents of input to output. Doesn't close either stream. </summary>
        public static void CopyStream(Stream input, Stream output) {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
                output.Write(buffer, 0, len);
            }
        }

        /// <summary>
        ///     Copies the contents of input to output. Doesn't close either stream. </summary>
        public static async Task CopyStreamAsync(Stream input, Stream output) {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = await input.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await output.WriteAsync(buffer, 0, len);
            }
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static string ReadFullyAsString(Stream input) {
            using (var sr = new StreamReader(input)) {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static async ValueTask<string> ReadFullyAsStringAsync(Stream input) {
            var sr = new StreamReader(input);
            return await sr.ReadToEndAsync();
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static void WriteString(Stream stream, string data) {
            using (var sw = new StreamWriter(stream)) {
                sw.Write(data);
            }
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static async Task WriteStringAsync(Stream stream, string data) {
            var sw = new StreamWriter(stream);
            await sw.WriteAsync(data);
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static async Task WriteAsync(Stream stream, byte[] data) {
            await stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        ///     Stream to bytes </summary>
        public static byte[] ReadFully(Stream input) {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream()) {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        ///     Stream to bytes </summary>
        public static async Task<byte[]> ReadFullyAsync(Stream input) {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream()) {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                    await ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     File to bytes </summary>
        public static byte[] ReadFully(string fullPath) {
            return File.ReadAllBytes(fullPath);
        }

        /// <summary>
        ///     Compares two arrays </summary>
        public static bool Compare(byte[] bin1, byte[] bin2) {
            if (bin1.Length != bin2.Length) {
                return false;
            }
            for (int i = 0; i < bin1.Length; i++) {
                if (bin1[i] != bin2[i]) {
                    return false;
                }
            }
            return true;
        }

        public static byte[] GetBytes(string str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string ToHex(this byte[] bytes, bool upperCase = false) {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        public static byte[] HexToBytes(this string hex) {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] Combine(params byte[][] arrays) {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays) {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        /// <summary>
        ///     Converts Bytes to Ints </summary>
        public static int[] BytesToInts(this byte[] bytes, int destinationLength) {
            Array.Resize(ref bytes, destinationLength * 4);

            int[] ints = new int[(int)(bytes.Length / 4)];
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
            for (var i = 0; i < ints.Length; i ++) {
                bytes[i] = BitConverter.GetBytes(ints[i]);
            }
            return Combine(bytes);
        }
    }
}