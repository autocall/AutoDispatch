using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Custom status в модалке (V1 CustomStatusController.CustomStatusModel): EntityId = Guid.Empty — новый; длина — колонка Name </summary>
public class CustomStatusFormViewModel {
    public Guid EntityId { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; }
}
