using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoCall.Common.Extensions {
    public static class JsonExtension {
        public static string SerializeObject<T>(T obj, JsonConverter converter) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);
            var result = JsonConvert.SerializeObject(obj, settings);
            return result;
        }

        public static string SerializeObject<T>(T obj, TypeNameHandling typeNameHandling = TypeNameHandling.All) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = typeNameHandling;
            settings.NullValueHandling = NullValueHandling.Ignore;
            var result = JsonConvert.SerializeObject(obj, settings);
            return result;
        }

        public static T DeserializeObject<T>(string str, JsonConverter converter) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(str, settings);
        }

        public static T DeserializeObject<T>(string str, TypeNameHandling TypeNameHandling) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(str, settings);
        }

        public static T DeserializeObject<T>(string str) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(str, settings);
        }

        public static T DeserializeObject<T>(Stream stream) {
            return DeserializeObject<T>(new StreamReader(stream));
        }

        public static T DeserializeObject<T>(StreamReader stream) {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(stream.ReadToEnd(), settings);
        }

        public static bool IsValidJson(string strInput) {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try {
                    var obj = JToken.Parse(strInput);
                    return true;
                } catch (JsonReaderException jex) {
                    return false;
                } catch (Exception ex)
                  {
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}
