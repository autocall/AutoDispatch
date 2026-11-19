using System;
using System.Linq;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Контакты: JSON-строка Dto ↔ ContactsViewModel (V1 Contacts/DtoToVm, Contacts/VmToDto).
    ///     ToVm(null) — пустой ContactsViewModel, как в V1 </summary>
    public static class ContactsConverter {

        public static ContactsViewModel ToVm(string json) {
            if (String.IsNullOrEmpty(json) == false) {
                try {
                    return new ContactsViewModel(JsonExtension.DeserializeObject<ContactBaseViewModel[]>(json).ToList());
                } catch (Exception e) {
                    _webl.e("Deserialize Contacts", e);
                }
            }

            var result = new ContactsViewModel();
            return result;
        }

        public static string ToDto(ContactsViewModel source) {
            if (source == null) return null;
            if (source.Contacts != null && source.Contacts.Any()) {
                return JsonExtension.SerializeObject(source.Contacts.ToArray());
            } else {
                return null;
            }
        }
    }
}
