using AutoCall.Logic.Settings.Sections;

namespace AutoCall.Logic.Contracts.Settings.Sections {
    public class BoardSettings2 {

        public double ZoomValue { get; set; } = 1;

        /// <summary>
        ///     Gets or sets offset splitter </summary>
        public double OffsetSplitter { get; set; } = 200;

        /// <summary>
        ///     Gets or sets is board enabled </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets records columns' settings </summary>
        public GridViewSettings RecordColumnSettings2 { get; set; }

        /// <summary>
        ///     Gets or sets patterns columns' settings </summary>
        public GridViewSettings PatternColumnSettings2 { get; set; }

        /// <summary>
        ///     NOTES: Old property, Required for correct parsing </summary>
        public byte[] RecordColumnSettings { get; set; }

        /// <summary>
        ///     NOTES: Old property, Required for correct parsing </summary>
        public byte[] PatternColumnSettings { get; set; }
    }
}