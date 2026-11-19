using System;
using AutoMapper;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Mappings {
    /// <summary>
    ///     AutoMapper 12 (AI/wiki/plan.md «DI и AutoMapper в App»). Из V1 убраны пары модуля AutoCall: записи лоадбордов
    ///     (DatPower, CHRWTrucks, JBHunt, Truckstop, Knight, EnglandLogistics, Bnsf, Coyote, XPOLogistics, ArcBest, Truckloads) → LoadViewModel,
    ///     DatPowerLocationViewModel → LocationViewModel, LocationViewModel → AutoCall.Logic.Dto.LocationDto, DatPowerRecordViewModel → SimilarLoadDto.
    ///     CarrierHistoryViewModel → RequestHistory2Dto — в BllMappingProfile (с ForMember LoadType; повторная пара в AutoMapper 12 — ошибка конфигурации) </summary>
    public class ViewMappingProfile : Profile {
        public ViewMappingProfile() {
            // ширина колонок грида в настройках (GridViewColumnSettings.Width double ↔ GridColumnSettingsViewModel.Width GridViewLength)
            CreateMap<double, GridViewLength>().ConvertUsing(e => Double.IsNaN(e) ? GridViewLength.Auto : new GridViewLength(e));
            CreateMap<GridViewLength, double>().ConvertUsing(e => e.IsAuto ? Double.NaN : e.Value);
        }
    }
}
