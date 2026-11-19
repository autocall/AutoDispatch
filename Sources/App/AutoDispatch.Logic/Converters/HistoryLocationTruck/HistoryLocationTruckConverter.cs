using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 HistoryLocationTruck/DtoToVm, HistoryLocationTruck/VmToDto </summary>
    public static class HistoryLocationTruckConverter {

        public static HistoryLocationTruckViewModel ToVm(HistoryLocationTruckDto source) {
            if (source == null) return null;
            var result = new HistoryLocationTruckViewModel();
            BaseConverter.FillHistoryLocationVm(source, result);
            result.TruckId = source.TruckId;
            return result;
        }

        public static HistoryLocationTruckDto ToDto(HistoryLocationTruckViewModel source) {
            if (source == null) return null;
            var result = new HistoryLocationTruckDto();
            BaseConverter.FillHistoryLocationDto(source, result);
            result.TruckId = source.TruckId;
            return result;
        }
    }
}
