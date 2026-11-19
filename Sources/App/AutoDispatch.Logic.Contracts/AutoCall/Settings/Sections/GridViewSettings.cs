using System.Collections.Generic;

namespace AutoCall.Logic.Settings.Sections {
    public class GridViewSettings {
        public List<GridViewColumnSettings> Columns { get; set; } = new List<GridViewColumnSettings>();
        public string SelectedFilterColumnsName { get; set; }
    }

    public class GridViewColumnSettings {
        public string UniqueName { get; set; }
        public bool IsVisible { get; set; }
        public double Width { get; set; }
        public int DisplayIndex { get; set; }
    }
}
