using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 DatDriver/DtoToVm, DatDriver/VmToDto (тела ConvertCore как есть) </summary>
    public static class DatDriverConverter {

        public static DatDriverViewModel ToVm(DatDriverDto source) {
            if (source == null) return null;
            var result = new DatDriverViewModel();
            BaseConverter.FillDispatchVm(source, result);

            result.Name = source.Name;
            result.TruckNumber = source.TruckNumber;
            result.TrailerNumber = source.TrailerNumber;
            result.ContactPhone = source.ContactPhone;
            result.Comments = source.Comments;
            result.CarrierViewLate = source.CarrierViewLate;
            result.CarrierViewStatus = source.CarrierViewStatus;
            if (source.CarrierViewStatusesJson != null) {
                result.CarrierViewStatuses = JsonExtension.DeserializeObject<List<string>>(source.CarrierViewStatusesJson, TypeNameHandling.None);
            }
            result.CarrierViewId = source.CarrierViewId;
            result.CarrierViewLoadId = source.CarrierViewLoadId;
            result.CarrierViewLink = source.CarrierViewLink;
            result.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
            result.CarrierViewPickupComment = source.CarrierViewPickupComment;
            result.CarrierViewPickupId = source.CarrierViewPickupId;
            result.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
            result.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
            result.CarrierViewEdit = source.CarrierViewEdit;

            return result;
        }

        public static DatDriverDto ToDto(DatDriverViewModel source) {
            if (source == null) return null;
            var result = new DatDriverDto();
            BaseConverter.FillDispatchDto(source, result);

            result.Name = source.Name;
            result.TruckNumber = source.TruckNumber;
            result.TrailerNumber = source.TrailerNumber;
            result.ContactPhone = source.ContactPhone;
            result.Comments = source.Comments;
            result.CarrierViewLate = source.CarrierViewLate;
            result.CarrierViewStatus = source.CarrierViewStatus;
            result.CarrierViewStatusesJson = JsonExtension.SerializeObject<List<string>>(source.CarrierViewStatuses);
            result.CarrierViewId = source.CarrierViewId;
            result.CarrierViewLoadId = source.CarrierViewLoadId;
            result.CarrierViewLink = source.CarrierViewLink;
            result.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
            result.CarrierViewPickupComment = source.CarrierViewPickupComment;
            result.CarrierViewPickupId = source.CarrierViewPickupId;
            result.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
            result.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
            result.CarrierViewEdit = source.CarrierViewEdit;

            return result;
        }
    }
}
