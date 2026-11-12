using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AutoDispatch.Web.Common.Extensions;

public static class JsonExtension {

    public static string Serialize<T>(T obj,
        TypeNameHandling typeNameHandling = TypeNameHandling.None,
        NullValueHandling nullValueHandling = NullValueHandling.Ignore) {
        // camelCase — контракт JSON для App и htmx-ответов
        var settings = new JsonSerializerSettings {
            TypeNameHandling = typeNameHandling,
            NullValueHandling = nullValueHandling,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return JsonConvert.SerializeObject(obj, settings);
    }

    public static T Deserialize<T>(string str) {
        var settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        return JsonConvert.DeserializeObject<T>(str, settings);
    }
}
