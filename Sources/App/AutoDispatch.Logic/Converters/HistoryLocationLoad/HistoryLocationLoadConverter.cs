using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 HistoryLocationLoad/DtoToVm, HistoryLocationLoad/VmToDto </summary>
    public static class HistoryLocationLoadConverter {

        public static HistoryLocationLoadViewModel ToVm(HistoryLocationLoadDto source) {
            if (source == null) return null;
            var result = new HistoryLocationLoadViewModel();
            BaseConverter.FillHistoryLocationVm(source, result);
            result.HistoryLocationTrailerId = source.HistoryLocationTrailerId;
            result.LoadId = source.LoadId;
            return result;
        }

        public static HistoryLocationLoadDto ToDto(HistoryLocationLoadViewModel source) {
            if (source == null) return null;
            var result = new HistoryLocationLoadDto();
            BaseConverter.FillHistoryLocationDto(source, result);
            result.HistoryLocationTrailerId = source.HistoryLocationTrailerId;
            result.LoadId = source.LoadId;
            return result;
        }
    }
}
