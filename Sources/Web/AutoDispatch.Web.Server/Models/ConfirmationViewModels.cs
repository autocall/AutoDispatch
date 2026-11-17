using System.ComponentModel.DataAnnotations;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Logic.Dtos.Dispatch;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Страница /Confirmation?LoadId= (V1 ConfirmationModel): груз, карриер, компания и форма подписи — имя карриера </summary>
public class ConfirmationViewModel {

    [Required]
    [StringLength(24, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
    public string Name { get; set; }

    public Guid LoadId { get; set; }

    public LoadDto Load { get; set; }
    public DatCompanyDto Carrier { get; set; }
    public ActualCompanyDto Company { get; set; }

    /// <summary>
    ///     Ошибка подписи для фрагмента формы (V1 Notifier.warning) </summary>
    public string Error { get; set; }

    /// <summary>
    ///     Разбивка ставки, как в V1 Index.cshtml: доплаты с суммой, CarrierView tracking, line haul — только если
    ///     итог отличается от line haul; Rate — итог DispatchHelper.GetRate </summary>
    public List<GainViewModel> Gains { get; set; } = new();
    public decimal? Rate { get; set; }
    public string Currency { get; set; }
    public string Equipment { get; set; }
    public string Weight { get; set; }
    public string Length { get; set; }
    public string PickupAppointment { get; set; }
    public string DeliveryAppointment { get; set; }
    public string CarrierPhone { get; set; }
}
