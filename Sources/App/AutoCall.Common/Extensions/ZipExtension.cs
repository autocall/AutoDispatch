using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using AutoCall.Common.Helpers;
using Newtonsoft.Json;

namespace AutoCall.Common.Extensions {

    /// <summary>
    ///     UnZip JSON: 49.98 KB
    ///     UnZip Binary: 41.4 KB
    ///     Zip JSON: 15.3 KB
    ///     ZipBinary: 18.15 KB
    /// </summary>
    public static class ZipExtension {

        #region [ _private ]

        public static byte[] Compress(byte[] input) {
            using (var outStream = new MemoryStream()) {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var mStream = new MemoryStream(input))
                    mStream.CopyTo(tinyStream);

                return outStream.ToArray();
            }
        }

        public static async Task<byte[]> CompressAsync(byte[] input) {
            using (var outStream = new MemoryStream()) {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var mStream = new MemoryStream(input))
                    await mStream.CopyToAsync(tinyStream);

                return outStream.ToArray();
            }
        }

        private static byte[] Compress(Stream mStream) {
            if (mStream.CanSeek) {
                mStream.Seek(0, SeekOrigin.Begin);
            }
            using (var outStream = new MemoryStream()) {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                    mStream.CopyTo(tinyStream);

                var result = outStream.ToArray();
                return result;
            }
        }

        private static async Task<byte[]> CompressAsync(Stream mStream) {
            if (mStream.CanSeek) {
                mStream.Seek(0, SeekOrigin.Begin);
            }
            using (var outStream = new MemoryStream()) {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                    await mStream.CopyToAsync(tinyStream);

                var result = outStream.ToArray();
                return result;
            }
        }

        public static byte[] Decompress(byte[] input) {
            try {

                using (var inStream = new MemoryStream(input))
                using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
                using (var bigStreamOut = new MemoryStream()) {
                    bigStream.CopyTo(bigStreamOut);
                    return bigStreamOut.ToArray();
                }
            } catch (Exception e) {
                var data = Convert.ToBase64String(input);
                _l.e($"Decompress\n{data}");
                throw;
            }
        }

        public static async Task<byte[]> DecompressAsync(byte[] input) {
            using (var inStream = new MemoryStream(input))
            using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream()) {
                await bigStream.CopyToAsync(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }

        private static byte[] Decompress(Stream inStream) {
            if (inStream.CanSeek) {
                inStream.Position = 0;
            }
            using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream()) {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }

        private static async Task<byte[]> DecompressAsync(Stream inStream) {
            if (inStream.CanSeek) {
                inStream.Position = 0;
            }
            using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream()) {
                await bigStream.CopyToAsync(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }

        #endregion [ _private ]

        #region [ Public ]

        #region [ Store as Zip Binary ]

        //private static IFormatter formatter = new BinaryFormatter();

        //public static byte[] SerializeObject<T>(T obj) {
        //    byte[] bytes;
        //    using (MemoryStream memstream = new MemoryStream()) {
        //        formatter.Serialize(memstream, obj);
        //        bytes = Compress(memstream);
        //    }
        //    return bytes;
        //}

        //public static T DeserializeObject<T>(byte[] bytes) {
        //    bytes = Decompress(bytes);
        //    T wrapper;
        //    using (MemoryStream memstream = new MemoryStream(bytes)) {
        //        wrapper = (T)formatter.Deserialize(memstream);
        //    }
        //    return wrapper;
        //}

        //public static T DeserializeObject<T>(Stream stream) {
        //    var bytes = Decompress(stream);
        //    T wrapper;
        //    using (MemoryStream memstream = new MemoryStream(bytes)) {
        //        wrapper = (T)formatter.Deserialize(memstream);
        //    }
        //    return wrapper;
        //}

        #endregion [ Store as Zip Binary ]

        #region [ Store as Zip Json ]

        public static byte[] SerializeObject<T>(T obj, TypeNameHandling typeNameHandling = TypeNameHandling.All) {
            var str = JsonExtension.SerializeObject(obj, typeNameHandling);
            return Compress(BinaryHelper.GetBytes(str));
        }

        public static T DeserializeObject<T>(byte[] bytes) {
            bytes = Decompress(bytes);
            return JsonExtension.DeserializeObject<T>(BinaryHelper.GetString(bytes));
        }

        public static object DeserializeObjectJson(byte[] bytes) {
            bytes = Decompress(bytes);
            return JsonConvert.DeserializeObject(BinaryHelper.GetString(bytes));
        }

        public static async Task<T> DeserializeObjectAsync<T>(Stream stream) {
            var bytes = await DecompressAsync(stream);
            return JsonExtension.DeserializeObject<T>(BinaryHelper.GetString(bytes));
        }
        public static async Task<byte[]> SerializeObjectAsync<T>(T obj, TypeNameHandling typeNameHandling = TypeNameHandling.All) {
            var str = JsonExtension.SerializeObject(obj, typeNameHandling);
            return await CompressAsync(BinaryHelper.GetBytes(str));
        }

        public static async Task<T> DeserializeObjectAsync<T>(byte[] bytes) {
            bytes = await DecompressAsync(bytes);
            return JsonExtension.DeserializeObject<T>(BinaryHelper.GetString(bytes));
        }

        public static T DeserializeObject<T>(Stream stream) {
            var bytes = Decompress(stream);
            return JsonExtension.DeserializeObject<T>(BinaryHelper.GetString(bytes));
        }

        #endregion [ Store as Zip Json ]

        #endregion [ Public ]
    }
}