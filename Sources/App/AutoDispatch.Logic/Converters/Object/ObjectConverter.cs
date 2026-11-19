using System;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Объект по фактическому типу (V1 Object/DtoToVm, Object/VmToDto): та же цепочка if, вместо Mapper.Map — конкретный конвертер </summary>
    public static class ObjectConverter {

        public static BaseObjectViewModel ToVm(BaseDto source) {
            if (source == null) return null;
            if (source is DriverDto) {
                return DriverConverter.ToVm((DriverDto)source);
            } else if (source is TruckDto) {
                return TruckConverter.ToVm((TruckDto)source);
            } else if (source is TrailerDto) {
                return TrailerConverter.ToVm((TrailerDto)source);
            } else if (source is WarehouseDto) {
                return WarehouseConverter.ToVm((WarehouseDto)source);
            } else if (source is LoadDto) {
                return LoadConverter.ToVm((LoadDto)source);
            } else if (source is DatCompanyDto) {
                return DatCompanyConverter.ToVm((DatCompanyDto)source);
            } else if (source is DatDriverDto) {
                return DatDriverConverter.ToVm((DatDriverDto)source);
            } else if (source is SetupPacketDto) {
                return SetupPacketConverter.ToVm((SetupPacketDto)source);
            } else if (source is ActualCompanyDto) {
                // withot IBaseEventDto
                return ActualCompanyConverter.ToVm((ActualCompanyDto)source);
            } else {
                throw new Exception("Undefined type of BaseObjectDto: " + source.GetType());
            }
        }

        public static BaseDto ToDto(BaseObjectViewModel source) {
            if (source == null) return null;
            if (source is DriverViewModel) {
                return DriverConverter.ToDto((DriverViewModel)source);
            } else if (source is TruckViewModel) {
                return TruckConverter.ToDto((TruckViewModel)source);
            } else if (source is TrailerViewModel) {
                return TrailerConverter.ToDto((TrailerViewModel)source);
            } else if (source is LoadViewModel) {
                return LoadConverter.ToDto((LoadViewModel)source);
            } else if (source is DatCompanyViewModel) {
                return DatCompanyConverter.ToDto((DatCompanyViewModel)source);
            } else if (source is DatDriverViewModel) {
                return DatDriverConverter.ToDto((DatDriverViewModel)source);
            } else {
                throw new Exception("Undefined type of BaseObjectViewModel: " + source.GetType());
            }
        }
    }
}
