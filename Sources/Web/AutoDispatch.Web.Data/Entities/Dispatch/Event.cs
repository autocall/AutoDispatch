using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Журнал событий компании для long-polling App: сервер читает max(Id) по компании, клиент — с lastEventId </summary>
public class Event {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid CompanyId { get; set; }

    public DateTime CreatedDate { get; set; }

    public byte Type { get; set; }

    public int? HistoryId { get; set; }

    [ForeignKey("HistoryId")]
    public HistoryObject HistoryObject { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }
}
