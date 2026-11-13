using System.Reflection;
using AutoMapper;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;
using AutoDispatch.Web.Logic.WebServices.Safer;

namespace AutoDispatch.Web.Logic.Mapping;

/// <summary>
///     Маппинг dispatch (V1 BllMappingProfile): простые пары ReverseMap плюс все ITypeConverter из Mapping.Dispatch
///     (регистрация по namespace через reflection, как в V1). ActualCompany* контракта ↔ DispatchCompany* </summary>
public class DispatchMappingProfile : Profile {
    public DispatchMappingProfile() {
        CreateMap<ShipperReceiverCompany, ShipperReceiverCompanyDto>().ReverseMap();
        CreateMap<CustomerCompany, CustomerCompanyDto>().ReverseMap();
        CreateMap<HistoryObject, HistoryObjectDto>().ReverseMap();
        CreateMap<Defect, DefectDto>().ReverseMap();
        CreateMap<DefectRepair, DefectRepairDto>().ReverseMap();
        CreateMap<CompanyCustomStatus, CompanyCustomStatusDto>().ReverseMap();
        CreateMap<DispatchCompanyCustom, ActualCompanyCustomDto>()
            .ForMember(d => d.ActualCompanyId, o => o.MapFrom(s => s.DispatchCompanyId))
            .ReverseMap()
            .ForMember(d => d.DispatchCompanyId, o => o.MapFrom(s => s.ActualCompanyId));
        CreateMap<SetupPacketFactoringCompany, SetupPacketFactoringCompanyDto>().ReverseMap();
        CreateMap<SetupPacketContact, SetupPacketContactDto>().ReverseMap();
        CreateMap<SetupPacketReference, SetupPacketReferenceDto>().ReverseMap();
        CreateMap<SetupPacketInsuranceAgency, SetupPacketInsuranceAgencyDto>().ReverseMap();
        CreateMap<SaferCompanyModel, CensusCompanyDto>().ConvertUsing<SaferCompany_ModelToDto>();

        var converterInterface = typeof(ITypeConverter<,>);
        var ns = typeof(TypeConverter<,>).Namespace;
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (type.Namespace != ns || type.IsClass == false || type.IsAbstract || type.IsGenericTypeDefinition) continue;
            var converterType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == converterInterface);
            if (converterType == null) continue;
            var map = GetType().GetMethod(nameof(CreateMap), Type.EmptyTypes)
                .MakeGenericMethod(converterType.GenericTypeArguments).Invoke(this, null);
            map.GetType().GetMethod("ConvertUsing", Type.EmptyTypes).MakeGenericMethod(type).Invoke(map, null);
        }
    }
}
