using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Настройка компании dispatch (V1 d_ActualCompanyCustom) </summary>
public class DispatchCompanyCustom : BaseEntity {

    public Guid DispatchCompanyId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }

    public int Type { get; set; }

    [MaxLength(256)]
    public string Params { get; set; }

    [ForeignKey("DispatchCompanyId")]
    public DispatchCompany DispatchCompany { get; set; }
}
