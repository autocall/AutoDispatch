using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Контакты карриера — JSON-массив ContactBaseViewModel клиента с $type AutoDispatch.Logic.Contracts (V1) </summary>
public static class DatCompanyContactHelper {

    public static bool CheckPhone(string phone) {
        if (phone != null) {
            var len = phone.Count(Char.IsDigit);
            if (len == 10 || len == 11) {
                return true;
            }
        }
        return false;
    }

    public static string OptimizePhone(string phone) {
        return new String(phone.Where(Char.IsDigit).ToArray());
    }

    public static bool? CheckNewPhone(string json, string phone) {
        try {
            var jObject = (JArray)JsonConvert.DeserializeObject(json);
            var any = jObject.Any(e => ((dynamic)e).Type == 0 && ((dynamic)e).Phone == phone);
            return any;
        } catch {
            return null;
        }
    }

    public static string AddAndGetPhone(string json, string phone, string contact) {
        try {
            var jContacts = (JArray)JsonConvert.DeserializeObject(json);
            var jPhone = new JObject();
            jPhone.Add(new JProperty("$type", "AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts.ContactPhoneViewModel, AutoDispatch.Logic.Contracts"));
            jPhone.Add(new JProperty("Type", "0"));
            jPhone.Add(new JProperty("Phone", phone));
            jPhone.Add(new JProperty("Contact", contact));

            jContacts.AddFirst(jPhone);
            return jContacts.ToString();
        } catch {
            return json;
        }
    }

    public static string CreateAndGetPhone(string phone, string ext, string contact) {
        var json = "{\"$type\":\"AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts.ContactBaseViewModel[], AutoDispatch.Logic.Contracts\",\"$values\":[]}";
        try {
            var jContacts = (JArray)JsonConvert.DeserializeObject(json);
            var jPhone = new JObject();
            jPhone.Add(new JProperty("$type", "AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts.ContactPhoneViewModel, AutoDispatch.Logic.Contracts"));
            jPhone.Add(new JProperty("Type", "0"));
            jPhone.Add(new JProperty("Phone", phone));
            jPhone.Add(new JProperty("Contact", contact));
            if (String.IsNullOrEmpty(ext) == false) {
                jPhone.Add(new JProperty("Ext", ext));
            }

            jContacts.AddFirst(jPhone);
            return jContacts.ToString();
        } catch {
            return json;
        }
    }
}
