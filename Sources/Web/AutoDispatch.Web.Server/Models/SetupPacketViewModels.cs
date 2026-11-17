using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Logic.Dtos.Dispatch;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Страница шага CarrierRegistration (V1 Web/Models/SetupPacketViewModel): карриер, компания, setup packet, модель шага,
///     режим и номер шага; порядок шагов (Enabled/Next/Prev/StepDisplay) — как в V1: 3, 8–12 выключены </summary>
public class SetupPacketViewModel {

    public Guid DatCompanyId { get; set; }
    public ActualCompanyDto Company { get; set; }
    public DatCompanyDto Carrier { get; set; }
    public SetupPacketDto SetupPacket { get; set; }
    /// <summary>
    ///     Модель текущего шага (SetupPacketStepXXModel) — поля формы; view приводит к своему типу </summary>
    public object SetupPacketStepModel { get; set; }
    /// <summary>
    ///     View/Edit — страница агента (навигация по шагам), User — карриера </summary>
    public PageModeEnum Mode { get; set; }
    /// <summary>
    ///     Шаг страницы </summary>
    public byte Step { get; set; }
    /// <summary>
    ///     Ошибка сохранения шага (InfoException) — показывается над кнопками </summary>
    public string Error { get; set; }

    /// <summary>
    ///     Справочники для select'ов (StateHelper, таблиц State/Country в БД нет) </summary>
    public List<SetupPacketStateDto> States { get; set; }
    public List<SetupPacketCountryDto> Countries { get; set; }
    public List<SetupPacketFactoringCompanyDto> FactoringCompanies { get; set; }

    public T Model<T>() where T : SetupPacketStepModel => (T)SetupPacketStepModel;

    public string Title(byte step) {
        return step switch {
            0 => "Confidant",
            1 => "Carrier Registration Service",
            2 => "Contact Information",
            3 => "Questionnaire",
            4 => "Agreement",
            5 => "Insurance Instructions",
            6 => "Producer Requirements",
            7 => "Electronic W9",
            8 => "Carrier Profile",
            9 => "Equipment Profile",
            10 => "References",
            11 => "W8 Upload",
            12 => "Supplier Diversity Information",
            13 => "Your Documents",
            14 => "Send Email",
            _ => "undefined"
        };
    }

    /// <summary>
    ///     Шаги, которые проходит карриер (V1): questionnaire, carrier/equipment profile, references, W8, diversity выключены </summary>
    public static bool Enabled(byte step) {
        return step switch {
            3 or 8 or 9 or 10 or 11 or 12 => false,
            _ => true
        };
    }

    public static readonly byte[] EnabledSteps = Enumerable.Range(0, 15).Select(i => (byte)i).Where(Enabled).ToArray();

    /// <summary>
    ///     «3/9 complete» — номер среди включённых шагов (14 — Send Email — не считается, как в V1) </summary>
    public string StepDisplay(byte step) {
        var current = 0;
        var total = 0;
        for (byte i = 0; i < 14; i++) {
            if (Enabled(i)) {
                total++;
                if (i < step) {
                    current++;
                }
            }
        }
        return $"{current + 1}/{total}";
    }

    public byte Next(byte step) {
        while (Enabled(++step) == false) { }
        return step;
    }

    public byte Prev(byte step) {
        while (Enabled(--step) == false) { }
        return step;
    }

    public string StateCode(byte? stateId) => States?.FirstOrDefault(s => s.EntityId == stateId)?.Code;
}

/// <summary>
///     Блок контакта на шагах 0 и 2 (_ContactPart): поля с именами Prefix.Contact … для model binding;
///     Optional — чекбокс HasContact, SameAs — select «same as» (имя поля и варианты: значение → подпись) </summary>
public class SetupPacketContactPartModel {
    public string Prefix { get; set; }
    public SetupPacketContactDto Contact { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool Optional { get; set; }
    public string SameAsName { get; set; }
    public string SameAsValue { get; set; }
    public (string Value, string Text)[] SameAsOptions { get; set; } = Array.Empty<(string, string)>();
}

/// <summary>
///     Блок страхового агентства на шаге 6 (_InsuranceAgencyPart) </summary>
public class SetupPacketInsurancePartModel {
    public string Prefix { get; set; }
    public SetupPacketInsuranceAgencyDto Agency { get; set; }
    public string Title { get; set; }
    /// <summary>
    ///     Подпись чекбокса HasInsurance; null — без чекбокса (Auto, Cargo — всегда) </summary>
    public string Checkbox { get; set; }
    public string SameAsName { get; set; }
    public string SameAsValue { get; set; }
    public (string Value, string Text)[] SameAsOptions { get; set; } = Array.Empty<(string, string)>();
    /// <summary>
    ///     Партиал, видимый при снятом чекбоксе (отказ workers compensation), с моделью страницы </summary>
    public string HiddenPartial { get; set; }
    public SetupPacketViewModel Page { get; set; }
}
