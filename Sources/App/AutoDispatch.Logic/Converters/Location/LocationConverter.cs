using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Location/DtoToVm, Location/VmToDto </summary>
    public static class LocationConverter {

        public static LocationViewModel ToVm(LocationDto source) {
            if (source == null) return null;
            return new LocationViewModel() { Latitude = source.Latitude, Longitude = source.Longitude, FormattedAddress = source.FormattedAddress };
        }

        public static LocationDto ToDto(LocationViewModel source) {
            if (source == null) return null;
            return new LocationDto() { Latitude = source.Latitude, Longitude = source.Longitude, FormattedAddress = source.FormattedAddress };
        }
    }
}
