using System.IO.Compression;
using Newtonsoft.Json;

namespace AutoDispatch.Web.Common.Extensions;

/// <summary>
///     Снимки HistoryObject.Data и LoadLoadsZip: JSON с $type (DispatchJson) в UTF-16 (BinaryHelper.GetBytes) + gzip,
///     как ZipExtension V1 — формат читает и старый клиент </summary>
public static class ZipExtension {

    public static byte[] Compress(byte[] input) {
        using var outStream = new MemoryStream();
        using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
        using (var mStream = new MemoryStream(input)) {
            mStream.CopyTo(tinyStream);
        }
        return outStream.ToArray();
    }

    public static byte[] Decompress(byte[] input) {
        using var inStream = new MemoryStream(input);
        using var bigStream = new GZipStream(inStream, CompressionMode.Decompress);
        using var bigStreamOut = new MemoryStream();
        bigStream.CopyTo(bigStreamOut);
        return bigStreamOut.ToArray();
    }

    public static byte[] SerializeObject<T>(T obj, TypeNameHandling typeNameHandling = TypeNameHandling.All) {
        var str = DispatchJson.Serialize(obj, typeNameHandling);
        return Compress(BinaryHelper.GetBytes(str));
    }

    public static T DeserializeObject<T>(byte[] bytes) {
        bytes = Decompress(bytes);
        return DispatchJson.Deserialize<T>(BinaryHelper.GetString(bytes));
    }

    public static Task<byte[]> SerializeObjectAsync<T>(T obj, TypeNameHandling typeNameHandling = TypeNameHandling.All) {
        return Task.FromResult(SerializeObject(obj, typeNameHandling));
    }

    public static Task<T> DeserializeObjectAsync<T>(byte[] bytes) {
        return Task.FromResult(DeserializeObject<T>(bytes));
    }
}
