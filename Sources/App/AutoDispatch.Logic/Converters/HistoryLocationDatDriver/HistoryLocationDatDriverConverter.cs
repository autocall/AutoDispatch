using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 HistoryLocationDatDriver/DtoToVm, HistoryLocationDatDriver/VmToDto </summary>
    public static class HistoryLocationDatDriverConverter {

        public static HistoryLocationDatDriverViewModel ToVm(HistoryLocationDatDriverDto source) {
            if (source == null) return null;
            var result = new HistoryLocationDatDriverViewModel();
            BaseConverter.FillHistoryLocationVm(source, result);
            result.DatDriverId = source.DatDriverId;
            return result;
        }

        public static HistoryLocationDatDriverDto ToDto(HistoryLocationDatDriverViewModel source) {
            if (source == null) return null;
            var result = new HistoryLocationDatDriverDto();
            BaseConverter.FillHistoryLocationDto(source, result);
            result.DatDriverId = source.DatDriverId;
            return result;
        }
    }
}
