namespace AutoCall.Logic.Contracts.Settings.Sections {
    /// <summary>
    ///     Represents settings for column from data grid </summary>
    public class GridColumnSettings {
        /// <summary>
        ///     Gets or sets is column enabled </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets column width </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Gets or sets column order index </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Constructor </summary>
        public GridColumnSettings() {
        }

        /// <summary>
        ///     Constructor </summary>
        /// <param name="enabled">Is column enabled</param>
        /// <param name="width">Column width</param>
        /// <param name="index">Column order index</param>
        public GridColumnSettings(bool enabled, int width, int index) {
            Enabled = enabled;
            Width = width;
            Index = index;
        }
    }
}