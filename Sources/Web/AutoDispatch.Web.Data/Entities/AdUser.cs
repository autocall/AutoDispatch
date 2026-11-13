using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Data.Entities;

public class AdUser : IdentityUser<Guid> {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public override Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    /// <summary>
    ///     IANA time zone name </summary>
    [Column(TypeName = "varchar(64)")]
    public string TimeZoneId { get; set; }

    /// <summary>
    ///     Подпись агента на confirmation (V1 Users.Signature, ConfirmedSend). Новая колонка IdentityUsers —
    ///     нужна миграция и перенос из autocall.dbo.Users.Signature (AI/wiki/plan.md) </summary>
    [Column(TypeName = "varchar(64)")]
    public string Signature { get; set; }

    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime CreatedDateTime { get; set; }
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ModifiedDateTime { get; set; }

    public Guid? CreatedId { get; set; }
    public Guid? ModifiedId { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; }

    public static Guid SystemUserId = new Guid("00000000-0000-0000-0000-000000001000");
    public static Guid AdminUserId = new Guid("00000000-0000-0000-0000-000000001001");

    [NotMapped]
    public static AdUser SystemUser => new AdUser {
        Id = SystemUserId,
        CompanyId = Company.SystemCompanyId,
        CreatedId = SystemUserId,
        ModifiedId = SystemUserId,
        Email = AppSettings.SystemEmail,
        UserName = AppSettings.SystemUserName,
        CreatedDateTime = DateTime.UtcNow,
        ModifiedDateTime = DateTime.UtcNow,
    };

    [NotMapped]
    public static AdUser AdminUser => new AdUser {
        Id = AdminUserId,
        CompanyId = Company.SystemCompanyId,
        CreatedId = SystemUserId,
        ModifiedId = SystemUserId,
        Email = AppSettings.AdminEmail,
        UserName = AppSettings.AdminUserName,
        CreatedDateTime = DateTime.UtcNow,
        ModifiedDateTime = DateTime.UtcNow,
    };
}
