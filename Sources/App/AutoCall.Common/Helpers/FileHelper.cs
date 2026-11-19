using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Common.Helpers {
    public static class FileHelper {
        /// <summary>
        /// This is the same default buffer size as
        /// <see cref="StreamReader"/> and <see cref="FileStream"/>.
        /// </summary>
        private const int DefaultBufferSize = 4096;

        /// <summary>
        /// Indicates that
        /// 1. The file is to be used for asynchronous reading.
        /// 2. The file is to be accessed sequentially from beginning to end.
        /// </summary>
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        public static async ValueTask<string> ReadAllTextAsync(string path) {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
                return await BinaryHelper.ReadFullyAsStringAsync(stream);
        }
        public static async Task<byte[]> ReadAllBytesAsync(string path) {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
                return await BinaryHelper.ReadFullyAsync(stream);
        }


        public static async Task WriteAllBytesAsync(string path, byte[] data) {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, DefaultBufferSize, DefaultOptions))
                await BinaryHelper.WriteAsync(stream, data);
        }
    }
}
