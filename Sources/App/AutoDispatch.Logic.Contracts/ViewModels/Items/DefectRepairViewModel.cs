using System;
using System.Collections.ObjectModel;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class DefectRepairViewModel : BaseViewModel, ICloneable {
        public DateTime Date { get; set; }
        public decimal? Cost { get; set; }
        public string Comment { get; set; }

        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();

        public object Clone() {
            return new DefectRepairViewModel() {
                EntityId = this.EntityId,
                IsDeleted = this.IsDeleted,
                CreatedDate = this.CreatedDate,
                CreatedById = this.CreatedById,
                LastModifiedDate = this.LastModifiedDate,
                LastModifiedById = this.LastModifiedById,
                Date = this.Date,
                Cost = this.Cost,
                Comment = this.Comment,
                Files = new ObservableCollection<string>(this.Files),
            };
        }
    }
}
