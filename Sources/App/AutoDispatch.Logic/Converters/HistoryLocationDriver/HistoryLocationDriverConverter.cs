using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 HistoryLocationDriver/DtoToVm, HistoryLocationDriver/VmToDto </summary>
    public static class HistoryLocationDriverConverter {

        public static HistoryLocationDriverViewModel ToVm(HistoryLocationDriverDto source) {
            if (source == null) return null;
            var result = new HistoryLocationDriverViewModel();
            BaseConverter.FillHistoryLocationVm(source, result);
            result.HistoryLocationTruckId = source.HistoryLocationTruckId;
            result.DriverId = source.DriverId;
            return result;
        }

        public static HistoryLocationDriverDto ToDto(HistoryLocationDriverViewModel source) {
            if (source == null) return null;
            var result = new HistoryLocationDriverDto();
            BaseConverter.FillHistoryLocationDto(source, result);
            result.HistoryLocationTruckId = source.HistoryLocationTruckId;
            result.DriverId = source.DriverId;
            return result;
        }
    }
}
