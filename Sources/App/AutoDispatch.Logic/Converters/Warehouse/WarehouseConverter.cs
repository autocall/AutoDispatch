using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Warehouse/DtoToVm, Warehouse/VmToDto </summary>
    public static class WarehouseConverter {

        public static WarehouseViewModel ToVm(WarehouseDto source) {
            if (source == null) return null;
            var result = new WarehouseViewModel();
            BaseConverter.FillVm(source, result);

            result.Name = source.Name;
            result.City = source.City;
            result.Address = source.Address;
            result.Location = LocationConverter.ToVm(source.Location);

            return result;
        }

        public static WarehouseDto ToDto(WarehouseViewModel source) {
            if (source == null) return null;
            var result = new WarehouseDto();
            BaseConverter.FillDto(source, result);

            result.Name = source.Name;
            result.City = source.City;
            result.Address = source.Address;
            result.Location = LocationConverter.ToDto(source.Location);

            return result;
        }
    }
}
