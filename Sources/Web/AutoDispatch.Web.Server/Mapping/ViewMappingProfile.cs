using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AutoDispatch.Web.Logic.Dtos;
using AutoDispatch.Web.Logic.Dtos.Identity;
using AutoDispatch.Web.Server.Converters;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Mapping;

/// <summary>
///     ViewModel → Dto; сюда же конвертер ModelState для JsonFail(GetErrors()) </summary>
public class ViewMappingProfile : Profile {
    public ViewMappingProfile() {
        CreateMap<ModelStateDictionary, Dictionary<string, string>>().ConvertUsing<ModelState2ErrorsConverter>();

        CreateMap<CompanyFormViewModel, CreateCompanyDto>();
        CreateMap<CompanyFormViewModel, UpdateCompanyDto>();
        CreateMap<CompanyDto, CompanyFormViewModel>();

        CreateMap<UserFormViewModel, CreateUserDto>();
        CreateMap<UserFormViewModel, UpdateUserDto>();
        CreateMap<UserDto, UserFormViewModel>();
    }
}
