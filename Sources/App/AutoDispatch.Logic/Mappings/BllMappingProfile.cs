using AutoCall.Logic.Settings.Sections;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;

namespace AutoDispatch.Logic.Mappings {
    /// <summary>
    ///     AutoMapper 12 — только автоматические пары, как в AutoCall V3 (AI/wiki/plan.md «DI и AutoMapper в App»).
    ///     Все ConvertUsing V1 заменены статическими конвертерами AutoDispatch.Logic.Converters (LoadConverter.ToVm и т. п.);
    ///     вложенные автоматические пары V1 (SetupPacketInsuranceAgency, FactoringCompany, Defect, DefectRepair) — тоже статические.
    ///     DatPowerRecordViewModel → ContactBaseViewModel (модуль AutoCall) не перенесено </summary>
    public class BllMappingProfile : Profile {
        public BllMappingProfile() {
            CreateMap<QueryLoadViewModel, QueryLoadDto>().ReverseMap();

            CreateMap<HistoryObjectDto, HistoryObjectViewModel>().ReverseMap();

            CreateMap<CensusCompanyDto, CensusCompanyViewModel>().ReverseMap();
            CreateMap<CensusCompanySearchDto, CensusCompanySearchViewModel>().ReverseMap();

            CreateMap<GridViewSettings, GridSettingsViewModel>().ReverseMap();
            CreateMap<GridViewColumnSettings, AutoDispatch.Logic.ViewModels.GridColumnSettingsViewModel>().ReverseMap();

            CreateMap<CarrierHistoryViewModel, RequestHistory2Dto>().ForMember(dest => dest.LoadType, opt => opt.MapFrom(src => (int?)src.LoadType));
        }
    }
}
