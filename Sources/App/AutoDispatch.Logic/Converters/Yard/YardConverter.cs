using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Yard/DtoToVm, Yard/VmToDto </summary>
    public static class YardConverter {

        public static YardViewModel ToVm(YardDto source) {
            if (source == null) return null;
            var result = new YardViewModel();
            BaseConverter.FillVm(source, result);

            result.Name = source.Name;
            result.City = source.City;
            result.Address = source.Address;
            result.Location = LocationConverter.ToVm(source.Location);

            return result;
        }

        public static YardDto ToDto(YardViewModel source) {
            if (source == null) return null;
            var result = new YardDto();
            BaseConverter.FillDto(source, result);

            result.Name = source.Name;
            result.City = source.City;
            result.Address = source.Address;
            result.Location = LocationConverter.ToDto(source.Location);

            return result;
        }
    }
}
