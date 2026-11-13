using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Запись истории положения объекта: int identity, ссылка на снимок и событие </summary>
public class BaseHistoryEntity {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public virtual int HistoryId { get; set; }

    public virtual int EventId { get; set; }

    [Required, Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    public Guid CreatedById { get; set; }

    [ForeignKey("HistoryId")]
    public HistoryObject HistoryObject { get; set; }
}
