using System.Windows.Media.Imaging;

namespace AutoCall.WPF.Extensions.Core {
    /// <summary>
    ///     Represents information for data grid images column </summary>
    public class DataGridImageColumnInfo {
        /// <summary>
        ///     Gets or sets image control name </summary>
        public string ControlName { get; set; }

        /// <summary>
        ///     Gets or sets binding property </summary>
        public string Property { get; set; }

        /// <summary>
        ///     Gets or sets bitmap </summary>
        public BitmapImage Image { get; set; }
    }
}