using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Базовая сущность dispatch V1 (Entities/Base/BaseEntity.cs): Guid EntityId, аудит, soft delete.
///     Навигаций CreatedBy/LastModifiedBy на IdentityUsers нет (как у Company V3): авторы могут быть не перенесены </summary>
public class BaseEntity {

    [Key]
    public Guid EntityId { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; }

    public Guid CreatedById { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime LastModifiedDate { get; set; }

    public Guid LastModifiedById { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    public BaseEntity() {
        this.EntityId = Guid.NewGuid();
    }
}
