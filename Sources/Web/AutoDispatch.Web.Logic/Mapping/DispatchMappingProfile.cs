using AutoMapper;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Dtos.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping;

/// <summary>
///     Маппинг dispatch (V1 BllMappingProfile): только автоматические пары (ReverseMap, простой ForMember).
///     Конвертеры с реализацией — статические XConverter в Mapping.Dispatch, здесь не регистрируются (решено 2026-11-18).
///     ActualCompany* контракта ↔ DispatchCompany* </summary>
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

        // шаги CarrierRegistration (V1 BllMappingProfile): модель шага ↔ SetupPacketDto (у шага 13 — только Step, файлы сохраняет контроллер)
        CreateMap<SetupPacketStep00Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep01Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep02Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep03Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep04Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep05Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep06Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep07Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep08Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep09Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep10Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep11Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep12Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep13Model, SetupPacketDto>().ReverseMap();
        CreateMap<SetupPacketStep14Model, SetupPacketDto>().ReverseMap();
    }
}
