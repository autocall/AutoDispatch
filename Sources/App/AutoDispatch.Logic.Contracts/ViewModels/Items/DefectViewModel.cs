using System;
using System.Collections.ObjectModel;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class DefectViewModel : BaseViewModel, ICloneable {
        public Guid? TrailerId { get; set; }
        public Guid? TruckId { get; set; }
        public Guid? RepairId { get; set; }
        public int DefectId { get; set; }
        public string Description { get; set; }
        public bool ServiceNeeded { get; set; }

        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();
        public DefectRepairViewModel Repair { get; set; }

        public object Clone() {
            return new DefectViewModel() {
                EntityId = this.EntityId,
                IsDeleted = this.IsDeleted,
                CreatedDate = this.CreatedDate,
                CreatedById = this.CreatedById,
                LastModifiedDate = this.LastModifiedDate,
                LastModifiedById = this.LastModifiedById,
                TrailerId = this.TrailerId,
                TruckId = this.TruckId,
                RepairId = this.RepairId,
                DefectId = this.DefectId,
                Description = this.Description,
                ServiceNeeded = this.ServiceNeeded,
                Files = new ObservableCollection<string>(this.Files),
                Repair = (DefectRepairViewModel)this.Repair?.Clone(),
            };
        }
    }
}
