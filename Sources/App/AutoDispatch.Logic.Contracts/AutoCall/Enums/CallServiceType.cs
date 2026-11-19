using System.ComponentModel;

namespace AutoCall.Logic.Enums {
    /// <summary>
    ///     Софтфоны, как в AutoCall V3 (AI/wiki/plan.md «Софтфоны в App»): значения V1 сохранены — по ним читается перенесённая
    ///     секция CallServiceSettings. Skype(0), RingCentral(1), Bria(2), Talk(4), Vodia(5), Desktop(7) V1 не перенесены:
    ///     SettingsService.GetCallServiceSettings заменяет их на AbtoPhone, как V1 заменял устаревшие </summary>
    public enum CallServiceType {
        [Description("None")]
        Unknown = 3,
        [Description("AutoPhone")]
        MicroSIP = 6,
        [Description("AutoPhone v.2")]
        AbtoPhone = 8,
    }
}
