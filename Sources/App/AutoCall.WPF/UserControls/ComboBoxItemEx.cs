using System.ComponentModel;
using System.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    /// <summary>
    /// Extends the <see cref="ComboBoxItem"/> class.
    /// </summary>
    [Browsable(false)]
    public class ComboBoxItemEx : ComboBoxItem {
        /// <summary>
        /// Gets or sets a <see cref="bool"/> value that indicates if this item is highlighted.
        /// </summary>
        public new bool IsHighlighted {
            get { return base.IsHighlighted; }
            set { base.IsHighlighted = value; }
        }
    }
}
