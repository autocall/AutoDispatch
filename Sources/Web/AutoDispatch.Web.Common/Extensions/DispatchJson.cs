using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AutoDispatch.Web.Common.Extensions;

/// <summary>
///     JSON контракта App↔Web и снимков HistoryObject (V1 JsonExtension.SerializeObject): TypeNameHandling.All, без camelCase.
///     Binder: типы System.Private.CoreLib пишутся как mscorlib — иначе клиент на .NET Framework не разрешит $type
///     коллекций (List`1[[...]], System.Private.CoreLib); на входе принимаются оба имени (AI/wiki/protocol.md) </summary>
public static class DispatchJson {

    private static readonly JsonSerializerSettings AllSettings = Create(TypeNameHandling.All);
    private static readonly JsonSerializerSettings AutoSettings = Create(TypeNameHandling.Auto);

    public static JsonSerializerSettings Create(TypeNameHandling typeNameHandling) {
        return new JsonSerializerSettings {
            TypeNameHandling = typeNameHandling,
            NullValueHandling = NullValueHandling.Ignore,
            SerializationBinder = new MscorlibBinder()
        };
    }

    public static string Serialize<T>(T obj) {
        return JsonConvert.SerializeObject(obj, AllSettings);
    }

    public static string Serialize<T>(T obj, TypeNameHandling typeNameHandling) {
        return JsonConvert.SerializeObject(obj, typeNameHandling == TypeNameHandling.All ? AllSettings : Create(typeNameHandling));
    }

    public static T Deserialize<T>(string json) {
        return JsonConvert.DeserializeObject<T>(json, AutoSettings);
    }

    public static T Deserialize<T>(string json, TypeNameHandling typeNameHandling) {
        return JsonConvert.DeserializeObject<T>(json, Create(typeNameHandling));
    }

    public static bool IsValidJson(string value) {
        if (value == null) return false;
        value = value.Trim();
        if ((value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("[") && value.EndsWith("]"))) {
            try {
                JToken.Parse(value);
                return true;
            } catch (Exception) {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    ///     Имя сборки core-типов как у .NET Framework; остальные сборки — как есть (AutoDispatch.Dtos и т.д.) </summary>
    public class MscorlibBinder : DefaultSerializationBinder {

        private const string CoreLib = "System.Private.CoreLib";
        private const string Mscorlib = "mscorlib";

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            base.BindToName(serializedType, out assemblyName, out typeName);
            if (assemblyName == CoreLib) {
                assemblyName = Mscorlib;
            }
            // generic-аргументы внутри typeName тоже несут имя сборки: Dictionary`2[[System.String, System.Private.CoreLib],...]
            if (typeName != null && typeName.Contains(CoreLib)) {
                typeName = typeName.Replace(CoreLib, Mscorlib);
            }
        }

        // mscorlib на .NET 10 — фасад с type forwarding, Type.GetType разрешает его сам: BindToType без изменений
    }
}
