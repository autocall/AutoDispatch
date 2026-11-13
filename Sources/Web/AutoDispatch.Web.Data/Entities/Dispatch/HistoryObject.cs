using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Снимок объекта (сериализованный DTO) для истории и Undo; на Id ссылаются HistoryId объектов </summary>
[Index(nameof(ReferenceId), Name = "IX_HistoryObject_ReferenceId")]
[Index(nameof(GroupId), Name = "IX_HistoryObject_GroupId")]
public class HistoryObject {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public byte[] Data { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; }

    [Required]
    public Guid ReferenceId { get; set; }

    /// <summary>
    ///     The Group is one logical saving. Using is Undo operation </summary>
    public Guid? GroupId { get; set; }

    public bool Undo { get; set; }
}
