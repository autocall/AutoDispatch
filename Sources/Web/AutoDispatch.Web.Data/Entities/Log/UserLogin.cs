using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Data.Entities.Log;

/// <summary>
///     Журнал входов (БД LogDataBaseName): успешные и неуспешные попытки с сайта и из App </summary>
public class UserLogin {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     null — email не найден </summary>
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string Email { get; set; }

    public LoginSourceEnum Source { get; set; }
    public bool Success { get; set; }

    [Column(TypeName = "varchar(45)")]
    public string Ip { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string UserAgent { get; set; }
}
