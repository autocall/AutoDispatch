using AutoCall.Common.Extensions;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.ViewModels;
using Newtonsoft.Json;

namespace AutoCall.Logic.Converters {
    /// <summary>
    ///     Настройки софтфона: CallServiceSettings ↔ SettingsCallServiceViewModel (V1 CallService_SettingsToVm/CallService_VmToSettings
    ///     модуля AutoCall). Статический конвертер (AI/wiki/plan.md «DI и AutoMapper в App») </summary>
    public static class CallServiceSettingsConverter {

        public static SettingsCallServiceViewModel ToVm(CallServiceSettings source) {
            if (source == null) {
                return null;
            }
            var result = new SettingsCallServiceViewModel();
            result.SelectedCallService = source.CallService;
            result.CallType = source.CallType;
            result.MaxPhoneLines = source.MaxPhoneLines;
            result.DialCountryCode = source.DialCountryCode;
            result.TransferNumber = source.TransferNumber;
            result.MicroSIPLogin = source.MicroSIPLogin;
            result.MicroSIPPassword = source.MicroSIPPassword;
            if (source.MicroSIPSettings != null) {
                try {
                    result.MicroSIPSettings = JsonExtension.DeserializeObject<MicroSIPSettingsViewModel>(source.MicroSIPSettings);
                } catch (System.Exception) {
                }
            }
            return result;
        }

        public static CallServiceSettings ToSettings(SettingsCallServiceViewModel source) {
            if (source == null) {
                return null;
            }
            var result = new CallServiceSettings();
            result.CallService = source.SelectedCallService;
            result.CallType = source.CallType;
            result.MaxPhoneLines = source.MaxPhoneLines;
            result.DialCountryCode = source.DialCountryCode;
            result.TransferNumber = source.TransferNumber;
            result.MicroSIPLogin = source.MicroSIPLogin;
            result.MicroSIPPassword = source.MicroSIPPassword;
            if (source.MicroSIPSettings != null) {
                try {
                    result.MicroSIPSettings = JsonExtension.SerializeObject(source.MicroSIPSettings, TypeNameHandling.None);
                } catch (System.Exception) {
                }
            }
            return result;
        }
    }
}
