using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

/// <summary>
///     Настройки пользователя по секциям, ключ (UserId, Section); в AutoDispatch используется только секция Map (карта /Map, как в AutoCall V3) </summary>
public class UserSetting {

    public int UserId { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string Section { get; set; }

    [Column(TypeName = "varchar(max)")]
    public string Json { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime ModifiedDateTime { get; set; }
}
