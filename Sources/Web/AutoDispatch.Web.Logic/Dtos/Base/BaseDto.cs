namespace AutoDispatch.Web.Logic.Dtos;

public abstract class BaseDto {
    public Guid Id { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public Guid ModifiedById { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class BaseCompanyDto : BaseDto {
    public Guid CompanyId { get; set; }
}
