using System.Collections.Generic;

namespace AutoCall.Logic.Settings.Sections {
    public class FiltersColumnsSettings {
        public byte Table { get; set; }
        public List<FilterColumnsSettings> Filters { get; set; }
    }

    public class FilterColumnsSettings {
        public string Name { get; set; }
        public Dictionary<byte, string> Columns { get; set; }
    }
}
