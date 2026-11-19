using System.Collections;
using System.Collections.Generic;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Behaviors {
    public class ShowAllFilteringBehavior : FilteringBehavior {
        public override IEnumerable<object> FindMatchingItems(string searchText, IList items, IEnumerable<object> escapedItems, string textSearchPath, TextSearchMode textSearchMode) {
            return ((IEnumerable<object>)items);
        }
    }
}
