using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Behaviors {
    public class ZipFilteringBehavior : FilteringBehavior {
        public override IEnumerable<object> FindMatchingItems(string searchText, IList items, IEnumerable<object> escapedItems, string textSearchPath, TextSearchMode textSearchMode) {
            if (searchText != null && searchText.Trim().All(e => char.IsDigit(e))) {
                return items.Cast<object>();
            } else {
                return base.FindMatchingItems(searchText, items, escapedItems, textSearchPath, textSearchMode);
            }
        }
    }
}
