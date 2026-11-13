using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

public abstract class BaseEntity : IBaseUpdateMap {

    /// <summary>
    ///     Guid, как в V1 (решено 2026-11-13, AI/wiki/plan.md): Guid.NewGuid() в репозитории перед вставкой </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime CreatedDateTime { get; set; }

    public virtual Guid CreatedById { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ModifiedDateTime { get; set; }

    public virtual Guid ModifiedById { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CreatedById")]
    public virtual AdUser CreatedBy { get; set; }

    [ForeignKey("ModifiedById")]
    public virtual AdUser ModifiedBy { get; set; }
}

public interface IBaseUpdateMap {
    DateTime ModifiedDateTime { get; set; }
    Guid ModifiedById { get; set; }
}
