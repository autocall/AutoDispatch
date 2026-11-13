namespace AutoDispatch.Web.Logic.Dtos;

public class CompanyDto : BaseDto {
    public string Name { get; set; }
}

public class CreateCompanyDto {
    public string Name { get; set; }
}

public class UpdateCompanyDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
}
