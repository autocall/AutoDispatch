using System;
using System.Collections.Generic;
using System.Linq;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.Converters;
using AutoDispatch.Logic.ViewModels.Items;
using AutoMapper;

namespace AutoDispatch.Logic.Heplers {
    internal static class ConverterHelper {
        /// <summary>
        ///     Finds property with TargetResultAttribute and converts to ViewModel.
        ///     V1 конвертировал через IMapper по типу T (ConvertUsing-пары BllMappingProfile); теперь — статические конвертеры по типу
        ///     DTO (AI/wiki/plan.md «DI и AutoMapper в App»), автоматические пары (HistoryObject) — IMapper. Список null → пустой список,
        ///     как делал AutoMapper </summary>
        internal static object ConvertEventResponseData<T>(IMapper mapper, BaseEventResponseData data) {
            var targetProperty = data.GetTargetProperty();
            if (targetProperty != null) {
                var target = data.GetTarget();
                if (typeof(T) != targetProperty.PropertyType) {
                    return ToViewModel(typeof(T), target, mapper);
                } else {
                    return target;
                }
            }
            return null;
        }

        private static object ToViewModel(Type type, object target, IMapper mapper) {
            switch (target) {
                case DriverDto dto: return DriverConverter.ToVm(dto);
                case TruckDto dto: return TruckConverter.ToVm(dto);
                case TrailerDto dto: return TrailerConverter.ToVm(dto);
                case WarehouseDto dto: return WarehouseConverter.ToVm(dto);
                case YardDto dto: return YardConverter.ToVm(dto);
                case LoadDto dto: return LoadConverter.ToVm(dto);
                case DatCompanyDto dto: return DatCompanyConverter.ToVm(dto);
                case DatDriverDto dto: return DatDriverConverter.ToVm(dto);
                case SetupPacketDto dto: return SetupPacketConverter.ToVm(dto);
                case FileDto dto: return FileConverter.ToVm(dto);
                case DefectDto dto: return DefectConverter.ToVm(dto);
                case DefectRepairDto dto: return DefectRepairConverter.ToVm(dto);
            }
            if (type == typeof(List<DriverViewModel>)) return ToList<DriverDto, DriverViewModel>(target, DriverConverter.ToVm);
            if (type == typeof(List<TruckViewModel>)) return ToList<TruckDto, TruckViewModel>(target, TruckConverter.ToVm);
            if (type == typeof(List<TrailerViewModel>)) return ToList<TrailerDto, TrailerViewModel>(target, TrailerConverter.ToVm);
            if (type == typeof(List<WarehouseViewModel>)) return ToList<WarehouseDto, WarehouseViewModel>(target, WarehouseConverter.ToVm);
            if (type == typeof(List<YardViewModel>)) return ToList<YardDto, YardViewModel>(target, YardConverter.ToVm);
            if (type == typeof(List<LoadViewModel>)) return ToList<LoadDto, LoadViewModel>(target, LoadConverter.ToVm);
            if (type == typeof(List<DatCompanyViewModel>)) return ToList<DatCompanyDto, DatCompanyViewModel>(target, DatCompanyConverter.ToVm);
            if (type == typeof(List<DatDriverViewModel>)) return ToList<DatDriverDto, DatDriverViewModel>(target, DatDriverConverter.ToVm);
            if (type == typeof(List<SetupPacketViewModel>)) return ToList<SetupPacketDto, SetupPacketViewModel>(target, SetupPacketConverter.ToVm);
            if (type == typeof(List<FileViewModel>)) return ToList<FileDto, FileViewModel>(target, FileConverter.ToVm);
            if (type == typeof(List<DefectViewModel>)) return ToList<DefectDto, DefectViewModel>(target, DefectConverter.ToVm);
            if (type == typeof(List<CustomerCompanyViewModel>)) return ToList<CustomerCompanyDto, CustomerCompanyViewModel>(target, CustomerCompanyConverter.ToVm);
            if (type == typeof(List<ShipperReceiverCompanyViewModel>)) return ToList<ShipperReceiverCompanyDto, ShipperReceiverCompanyViewModel>(target, ShipperReceiverCompanyConverter.ToVm);
            if (type == typeof(List<HistoryLocationDriverViewModel>)) return ToList<HistoryLocationDriverDto, HistoryLocationDriverViewModel>(target, HistoryLocationDriverConverter.ToVm);
            if (type == typeof(List<HistoryLocationTruckViewModel>)) return ToList<HistoryLocationTruckDto, HistoryLocationTruckViewModel>(target, HistoryLocationTruckConverter.ToVm);
            if (type == typeof(List<HistoryLocationTrailerViewModel>)) return ToList<HistoryLocationTrailerDto, HistoryLocationTrailerViewModel>(target, HistoryLocationTrailerConverter.ToVm);
            if (type == typeof(List<HistoryLocationLoadViewModel>)) return ToList<HistoryLocationLoadDto, HistoryLocationLoadViewModel>(target, HistoryLocationLoadConverter.ToVm);
            if (type == typeof(List<HistoryLocationDatDriverViewModel>)) return ToList<HistoryLocationDatDriverDto, HistoryLocationDatDriverViewModel>(target, HistoryLocationDatDriverConverter.ToVm);
            if (target == null) {
                return null;
            }
            // автоматические пары BllMappingProfile (HistoryObject)
            return mapper.Map(target, target.GetType(), type);
        }

        private static List<TViewModel> ToList<TDto, TViewModel>(object target, Func<TDto, TViewModel> convert) {
            return (target as IEnumerable<TDto>)?.Select(convert).ToList() ?? new List<TViewModel>();
        }
    }
}
