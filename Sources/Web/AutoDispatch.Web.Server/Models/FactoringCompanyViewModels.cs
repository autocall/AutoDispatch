using System.ComponentModel.DataAnnotations;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Factoring Companies (V1 FactoringCompany/Index): поиск и сортировка по колонке — в адресе страницы </summary>
public class FactoringCompaniesIndexViewModel {
    public string Search { get; set; }
    public string Sort { get; set; }
    public bool Desc { get; set; }
    public List<SetupPacketFactoringCompanyDto> Companies { get; set; }

    public static string StateCode(byte stateId) => StateHelper.GetState(stateId)?.Code;

    /// <summary>
    ///     Заголовок колонки: повторный клик по текущей — обратный порядок </summary>
    public string SortLink(string column) {
        var desc = Sort == column && Desc == false;
        var search = String.IsNullOrEmpty(Search) ? "" : $"&search={Uri.EscapeDataString(Search)}";
        return $"/Admin/FactoringCompanies?sort={column}&desc={desc.ToString().ToLower()}{search}";
    }

    public string SortMark(string column) => Sort == column ? (Desc ? " ▼" : " ▲") : "";
}

/// <summary>
///     Форма в модалке: EntityId = 0 — новая; обязательные поля как V1 FactoringCompanyModel.ts (Name, Address, City, State),
///     длины — колонки SetupPacketFactoringCompany. Страны в форме V1 нет — CountryId сохраняется как был (у всех 0, USA) </summary>
public class FactoringCompanyFormViewModel {
    public int EntityId { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; }

    [Required, MaxLength(64)]
    public string Address { get; set; }

    [Required, MaxLength(32)]
    public string City { get; set; }

    [Required(ErrorMessage = "The State field is required.")]
    public byte? StateId { get; set; }

    [MaxLength(16), Display(Name = "Zip")]
    public string ZipCode { get; set; }

    [MaxLength(64)]
    public string Email { get; set; }

    public byte CountryId { get; set; }
}
