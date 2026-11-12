namespace AutoDispatch.Web.Logic.Dtos;

public abstract class BaseDto {
    public int Id { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public int CreatedById { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public int ModifiedById { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class BaseCompanyDto : BaseDto {
    public int CompanyId { get; set; }
}
