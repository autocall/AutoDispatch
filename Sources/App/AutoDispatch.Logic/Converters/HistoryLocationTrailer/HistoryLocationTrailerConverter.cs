using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 HistoryLocationTrailer/DtoToVm, HistoryLocationTrailer/VmToDto </summary>
    public static class HistoryLocationTrailerConverter {

        public static HistoryLocationTrailerViewModel ToVm(HistoryLocationTrailerDto source) {
            if (source == null) return null;
            var result = new HistoryLocationTrailerViewModel();
            BaseConverter.FillHistoryLocationVm(source, result);
            result.TrailerId = source.TrailerId;
            return result;
        }

        public static HistoryLocationTrailerDto ToDto(HistoryLocationTrailerViewModel source) {
            if (source == null) return null;
            var result = new HistoryLocationTrailerDto();
            BaseConverter.FillHistoryLocationDto(source, result);
            result.TrailerId = source.TrailerId;
            return result;
        }
    }
}
