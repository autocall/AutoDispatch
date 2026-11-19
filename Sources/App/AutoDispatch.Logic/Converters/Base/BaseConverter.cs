using System;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Logic.ViewModels;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Общие поля конвертеров — базовые классы V1 BaseDtoToVm, BaseEventDtoToVm, BaseDispatchDtoToVm, BaseHistoryLocationDtoToVm
    ///     и обратные *VmToDto (AutoMapper TypeConverter.ConvertCore). Статические методы заполняют уже созданный destination
    ///     (AI/wiki/plan.md «DI и AutoMapper в App») </summary>
    public static class BaseConverter {

        #region [ Dto -> Vm ]

        /// <summary> V1 BaseDtoToVm </summary>
        public static void FillVm(BaseDto source, BaseViewModel destination) {
            destination.CreatedById = source.CreatedById;
            destination.CreatedByName = source.CreatedByName;
            destination.CreatedDate = ConvertToLocal(source.CreatedDate);
            destination.EntityId = source.EntityId;
            destination.IsDeleted = source.IsDeleted;
            destination.LastModifiedById = source.LastModifiedById;
            destination.LastModifiedByName = source.LastModifiedByName;
            destination.LastModifiedDate = ConvertToLocal(source.LastModifiedDate);
        }

        /// <summary> V1 BaseEventDtoToVm </summary>
        public static void FillEventVm<TDto>(TDto source, BaseObjectViewModel destination) where TDto : BaseDto, IBaseEventDto {
            FillVm(source, destination);
            destination.EventId = source.EventId;
        }

        /// <summary> V1 BaseDispatchDtoToVm (наследник BaseDtoToVm, EventId не заполняет — как в V1) </summary>
        public static void FillDispatchVm(BaseDispatchDto source, BaseDispatchViewModel destination) {
            FillVm(source, destination);
            destination.CurrentLocation = LocationConverter.ToVm(source.CurrentLocation);
        }

        /// <summary> V1 BaseHistoryLocationDtoToVm </summary>
        public static void FillHistoryLocationVm(BaseEventLocationDto source, BaseHistoryLoationViewModel destination) {
            destination.CreatedById = source.CreatedById;
            destination.CreatedDate = source.CreatedDate;
            destination.EventId = source.EventId;
            destination.HistoryId = source.HistoryId;
            destination.Id = source.Id;
            destination.Location = LocationConverter.ToVm(source.Location);
        }

        public static DateTime ConvertToLocal(DateTime dt) {
            return dt.ToLocalTime();
        }

        public static DateTime? ConvertToLocal(DateTime? dt) {
            if (dt == null) {
                return null;
            }
            return ConvertToLocal((DateTime)dt);
        }

        #endregion [ Dto -> Vm ]

        #region [ Vm -> Dto ]

        /// <summary> V1 BaseVmToDto </summary>
        public static void FillDto(BaseViewModel source, BaseDto destination) {
            destination.CreatedById = source.CreatedById;
            destination.CreatedDate = ConvertToUtc(source.CreatedDate);
            destination.EntityId = source.EntityId;
            destination.IsDeleted = source.IsDeleted;
            destination.LastModifiedById = source.LastModifiedById;
            destination.LastModifiedDate = ConvertToUtc(source.LastModifiedDate);
        }

        /// <summary> V1 BaseEventVmToDto: после BaseVmToDto даты перезаписываются без перевода в UTC — как в V1 </summary>
        public static void FillEventDto(BaseObjectViewModel source, BaseDto destination) {
            FillDto(source, destination);
            destination.CreatedById = source.CreatedById;
            destination.CreatedDate = source.CreatedDate;
            destination.EntityId = source.EntityId;
            destination.IsDeleted = source.IsDeleted;
            destination.LastModifiedById = source.LastModifiedById;
            destination.LastModifiedDate = source.LastModifiedDate;
        }

        /// <summary> V1 BaseDispatchVmToDto (наследник BaseVmToDto) </summary>
        public static void FillDispatchDto(BaseDispatchViewModel source, BaseDispatchDto destination) {
            FillDto(source, destination);
            destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLocation);
        }

        /// <summary> V1 BaseHistoryLocationVmToDto </summary>
        public static void FillHistoryLocationDto(BaseHistoryLoationViewModel source, BaseEventLocationDto destination) {
            destination.CreatedById = source.CreatedById;
            destination.CreatedDate = source.CreatedDate;
            destination.EventId = source.EventId;
            destination.HistoryId = source.HistoryId;
            destination.Id = source.Id;
            destination.Location = LocationConverter.ToDto(source.Location);
        }

        public static DateTime ConvertToUtc(DateTime dt) {
            return dt.ToUniversalTime();
        }

        public static DateTime? ConvertToUtc(DateTime? dt) {
            if (dt == null) {
                return null;
            }
            return ConvertToUtc((DateTime)dt);
        }

        #endregion [ Vm -> Dto ]
    }
}
