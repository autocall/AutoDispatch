using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Неисправности трака/трейлера. В V1 — автоматическая пара AutoMapper (DefectViewModel ↔ DefectDto, ReverseMap),
    ///     вызывалась из конвертеров Truck/Trailer и из UI; статическая, как остальные конвертеры (AI/wiki/plan.md «DI и AutoMapper в App»).
    ///     Почленно по совпадающим именам, как сопоставлял AutoMapper: null-список Files → пустой; CompanyId и Repaired DTO во вьюмодели нет </summary>
    public static class DefectConverter {

        public static DefectViewModel ToVm(DefectDto source) {
            if (source == null) return null;
            return new DefectViewModel() {
                EntityId = source.EntityId,
                CreatedDate = source.CreatedDate,
                CreatedById = source.CreatedById,
                CreatedByName = source.CreatedByName,
                LastModifiedDate = source.LastModifiedDate,
                LastModifiedById = source.LastModifiedById,
                LastModifiedByName = source.LastModifiedByName,
                IsDeleted = source.IsDeleted,
                TrailerId = source.TrailerId,
                TruckId = source.TruckId,
                RepairId = source.RepairId,
                DefectId = source.DefectId,
                Description = source.Description,
                ServiceNeeded = source.ServiceNeeded,
                Files = source.Files != null ? new ObservableCollection<string>(source.Files) : new ObservableCollection<string>(),
                Repair = DefectRepairConverter.ToVm(source.Repair),
            };
        }

        public static DefectDto ToDto(DefectViewModel source) {
            if (source == null) return null;
            return new DefectDto() {
                EntityId = source.EntityId,
                CreatedDate = source.CreatedDate,
                CreatedById = source.CreatedById,
                CreatedByName = source.CreatedByName,
                LastModifiedDate = source.LastModifiedDate,
                LastModifiedById = source.LastModifiedById,
                LastModifiedByName = source.LastModifiedByName,
                IsDeleted = source.IsDeleted,
                TrailerId = source.TrailerId,
                TruckId = source.TruckId,
                RepairId = source.RepairId,
                DefectId = source.DefectId,
                Description = source.Description,
                ServiceNeeded = source.ServiceNeeded,
                Files = source.Files != null ? new List<string>(source.Files) : new List<string>(),
                Repair = DefectRepairConverter.ToDto(source.Repair),
            };
        }
    }

    /// <summary> Ремонт неисправности. В V1 — автоматическая пара AutoMapper DefectRepairViewModel ↔ DefectRepairDto (см. DefectConverter) </summary>
    public static class DefectRepairConverter {

        public static DefectRepairViewModel ToVm(DefectRepairDto source) {
            if (source == null) return null;
            return new DefectRepairViewModel() {
                EntityId = source.EntityId,
                CreatedDate = source.CreatedDate,
                CreatedById = source.CreatedById,
                CreatedByName = source.CreatedByName,
                LastModifiedDate = source.LastModifiedDate,
                LastModifiedById = source.LastModifiedById,
                LastModifiedByName = source.LastModifiedByName,
                IsDeleted = source.IsDeleted,
                Date = source.Date,
                Cost = source.Cost,
                Comment = source.Comment,
                Files = source.Files != null ? new ObservableCollection<string>(source.Files) : new ObservableCollection<string>(),
            };
        }

        public static DefectRepairDto ToDto(DefectRepairViewModel source) {
            if (source == null) return null;
            return new DefectRepairDto() {
                EntityId = source.EntityId,
                CreatedDate = source.CreatedDate,
                CreatedById = source.CreatedById,
                CreatedByName = source.CreatedByName,
                LastModifiedDate = source.LastModifiedDate,
                LastModifiedById = source.LastModifiedById,
                LastModifiedByName = source.LastModifiedByName,
                IsDeleted = source.IsDeleted,
                Date = source.Date,
                Cost = source.Cost,
                Comment = source.Comment,
                Files = source.Files != null ? new List<string>(source.Files) : new List<string>(),
            };
        }
    }
}
