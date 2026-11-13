using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

/// <summary>
///     Компания-клиент (арендатор): пользователи и их данные фильтруются по CompanyId </summary>
public class Company : BaseEntity, ICompanyUpdateMap {

    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; }

    // Колонки автора остаются (аудит), но без FK на IdentityUsers: системная компания создаётся раньше первого пользователя
    [NotMapped]
    public override AdUser CreatedBy { get; set; }
    [NotMapped]
    public override AdUser ModifiedBy { get; set; }

    public static Guid SystemCompanyId = new Guid("00000000-0000-0000-0000-000000001000");
}

public interface ICompanyUpdateMap : IBaseUpdateMap {
    public string Name { get; set; }
}
