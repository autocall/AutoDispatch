using AutoMapper;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.Dtos;
using AutoDispatch.Web.Logic.Dtos.Identity;

namespace AutoDispatch.Web.Logic.Mapping;

public class LogicMappingProfile : Profile {
    public LogicMappingProfile() {
        CreateMap<AcUserDto, AcUser>().ReverseMap();
        CreateMap<AcRoleDto, AcRole>().ReverseMap();
        CreateMap<CompanyDto, Company>().ReverseMap();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<UpdateCompanyDto, Company>();
    }
}
