using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Data.Entities.Log;

/// <summary>
///     Онлайн-активность: одна строка на пользователя за интервал AppSettings.ActivityFlushInterval, в котором он делал запросы </summary>
public class UserActivity {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime SeenAt { get; set; }

    public LoginSourceEnum Source { get; set; }
}
