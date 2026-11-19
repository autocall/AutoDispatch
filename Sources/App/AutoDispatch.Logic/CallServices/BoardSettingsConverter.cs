using AutoCall.Logic.Contracts.Settings.Sections;
using AutoCall.Logic.ViewModels;

namespace AutoCall.Logic.Converters {
    /// <summary>
    ///     Настройки борда: BoardSettings2 ↔ BoardSettingsViewModel (V1 Board_SettingsToVm/Board_VmToSettings модуля AutoCall —
    ///     только Enabled и ZoomValue; OffsetSplitter и колонки при обратном преобразовании не переносятся, как в V1).
    ///     Статический конвертер (AI/wiki/plan.md «DI и AutoMapper в App») </summary>
    public static class BoardSettingsConverter {

        public static BoardSettingsViewModel ToVm(BoardSettings2 source) {
            if (source == null)
                return null;

            var destination = new BoardSettingsViewModel {
                Enabled = source.Enabled,
                ZoomValue = source.ZoomValue
            };

            return destination;
        }

        public static BoardSettings2 ToSettings(BoardSettingsViewModel source) {
            if (source == null)
                return null;

            var destination = new BoardSettings2 {
                Enabled = source.Enabled,
                ZoomValue = source.ZoomValue
            };

            return destination;
        }
    }
}
