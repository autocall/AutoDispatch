using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Объект компании со снимком истории (HistoryObject) </summary>
public class BaseObjectEntity : BaseEntity {

    /// <summary>
    ///     Current history object # </summary>
    public int HistoryId { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [ForeignKey("HistoryId")]
    public HistoryObject HistoryObject { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }
}
