using System.Text.RegularExpressions;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Телефон из полей FMCSA/Census (TEL_NUM, CELL_NUM, FAX_NUM) → контакт ContactPhoneViewModel: метод Convert(phone, contactName)
    ///     V1 DatPowerRecordVmToVm. Остальное в нём (контакт из записи DAT — DatPowerRecordViewModel) — модуль AutoCall, не перенесено.
    ///     Статический конвертер (AI/wiki/plan.md «DI и AutoMapper в App») </summary>
    public static class PhoneContactConverter {

        public static ContactBaseViewModel ToVm(string phone, string contactName = null) {
            var phoneViewModel = new ContactPhoneViewModel() { Contact = contactName };
            // Find extension
            Regex regex = new Regex(@"(\(\d{1,6}\))$");
            if (regex.IsMatch(phone)) {
                var ext = regex.Match(phone).Value;
                phone = phone.Replace(ext, "");
                var extStr = StringExtension.OnlyNumbers(ext);
                phoneViewModel.Ext = extStr;
            }
            var phoneStr = StringExtension.OnlyNumbers(phone);
            if (phoneStr.Length >= 10 && phoneStr.Length <= 11) {
                phoneViewModel.Phone = phoneStr;

            }
            return phoneViewModel;
        }
    }
}
