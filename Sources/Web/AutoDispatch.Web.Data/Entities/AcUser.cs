using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Data.Entities;

public class AcUser : IdentityUser<int> {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public override int Id { get; set; }

    public int CompanyId { get; set; }

    /// <summary>
    ///     IANA time zone name </summary>
    [Column(TypeName = "varchar(64)")]
    public string TimeZoneId { get; set; }

    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime CreatedDateTime { get; set; }
    [Required]
    [Column(TypeName = "smalldatetime")]
    public DateTime ModifiedDateTime { get; set; }

    public int? CreatedId { get; set; }
    public int? ModifiedId { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; }

    public static int SystemUserId = 1000;
    public static int AdminUserId = 1001;

    [NotMapped]
    public static AcUser SystemUser => new AcUser {
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
    public static AcUser AdminUser => new AcUser {
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
