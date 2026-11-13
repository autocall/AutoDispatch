using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Изменились ли дефект и ремонт по сравнению с БД (V1) </summary>
public static class CompareEntities {
    public static bool Compare(Defect entity1, Defect entity2) {
        return
            entity1.TrailerId == entity2.TrailerId &&
            entity1.TruckId == entity2.TruckId &&
            entity1.RepairId == entity2.RepairId &&
            entity1.DefectId == entity2.DefectId &&
            entity1.Description == entity2.Description &&
            entity1.ServiceNeeded == entity2.ServiceNeeded;
    }
    public static bool Compare(DefectRepair entity1, DefectRepair entity2) {
        return
            entity1.Date == entity2.Date &&
            entity1.Cost == entity2.Cost &&
            entity1.Comment == entity2.Comment;
    }
}
