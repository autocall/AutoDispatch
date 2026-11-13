using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class CustomerCompany : BaseEntity {

    [Required, MaxLength(64)]
    public string Name { get; set; }

    public string Contacts { get; set; }

    public Guid CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }
}
