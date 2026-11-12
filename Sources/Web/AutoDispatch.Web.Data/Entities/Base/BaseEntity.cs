using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

public abstract class BaseEntity : IBaseUpdateMap {

    /// <summary>
    ///     Ключ без IDENTITY: DbRandomHelper.NewInt32() в репозитории перед вставкой </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime CreatedDateTime { get; set; }

    public virtual int CreatedById { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ModifiedDateTime { get; set; }

    public virtual int ModifiedById { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CreatedById")]
    public virtual AcUser CreatedBy { get; set; }

    [ForeignKey("ModifiedById")]
    public virtual AcUser ModifiedBy { get; set; }
}

public interface IBaseUpdateMap {
    DateTime ModifiedDateTime { get; set; }
    int ModifiedById { get; set; }
}
