using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Trailer/DtoToVm, Trailer/VmToDto (тела ConvertCore как есть) </summary>
    public static class TrailerConverter {

        public static TrailerViewModel ToVm(TrailerDto source) {
            if (source == null) return null;
            var destination = new TrailerViewModel();
            BaseConverter.FillDispatchVm(source, destination);

            destination.Number = source.Number;
            destination.Type = (TrailerType)source.Type;
            destination.YardId = source.YardId;
            destination.History = source.History;

            if (source.Defects != null) {
                destination.Defects = new ObservableCollection<DefectViewModel>(
                    source.Defects.OrderByDescending(e => e.CreatedDate).Select(DefectConverter.ToVm));
            } else {
                destination.Defects = new ObservableCollection<DefectViewModel>();
            }

            #region [ Files ]

            destination.AnnualInspection_File = source.AnnualInspection_File;
            destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
            destination.Registration_File = source.Registration_File;
            destination.Maintenance_File = source.Maintenance_File;
            destination.Maintenance_Expiration = source.Maintenance_Expiration;

            destination.SchemeDate = BaseConverter.ConvertToLocal(source.SchemeDate);
            destination.SchemeFile = source.SchemeFile;

            #endregion [ Files ]

            #region [ BNSF ]

            destination.BnsfOwner = source.BnsfOwner;
            destination.BnsfShipper = source.BnsfShipper;
            destination.BnsfUnit = source.BnsfUnit;
            destination.BnsfLoadEmptyStatus = source.BnsfLoadEmptyStatus;
            destination.BnsfCurrentLocation = source.BnsfCurrentLocation;
            destination.BnsfEventCode = source.BnsfEventCode;
            destination.BnsfEventDate = source.BnsfEventDate;
            destination.BnsfETA_Deramp = source.BnsfETA_Deramp;
            destination.BnsfOrigin = source.BnsfOrigin;
            destination.BnsfDestination = source.BnsfDestination;
            destination.BnsfBeyondGoalHours = source.BnsfBeyondGoalHours;
            destination.BnsfTimeDeramp = source.BnsfTimeDeramp;
            destination.BnsfTimeNotify = source.BnsfTimeNotify;
            destination.BnsfFinalTrainArrival = source.BnsfFinalTrainArrival;
            destination.BnsfGoalDate = source.BnsfGoalDate;
            destination.BnsfNextevent = source.BnsfNextevent;
            destination.BnsfPickUpNumber = source.BnsfPickUpNumber;
            destination.BnsfStorageDay = source.BnsfStorageDay;
            destination.BnsfTrain = source.BnsfTrain;
            destination.BnsfTrucker = source.BnsfTrucker;
            destination.BnsfWaybillDate = source.BnsfWaybillDate;
            destination.BnsfRateAuthority = source.BnsfRateAuthority;

            var bnsfAmountDueString = new String(source.BnsfAmountDue?.Where(e => Char.IsDigit(e) || e == '.' || e == ',').ToArray());
            if (decimal.TryParse(bnsfAmountDueString, out var bnsfAmountDue) == true) {
                destination.BnsfAmountDue = bnsfAmountDue;
            }
            destination.BnsfDueDate = source.BnsfDueDate;
            destination.BnsfWaybillNumber = source.BnsfWaybillNumber;
            destination.BnsfBillNumber = source.BnsfBillNumber;
            destination.BnsfAdjustments = source.BnsfAdjustments;

            #endregion [ BNSF ]

            return destination;
        }

        public static TrailerDto ToDto(TrailerViewModel source) {
            if (source == null) return null;
            var destination = new TrailerDto();
            BaseConverter.FillDispatchDto(source, destination);

            destination.Number = source.Number;
            destination.Type = (byte)source.Type;
            destination.YardId = source.YardId;
            destination.History = source.History;

            destination.Defects = source.Defects?.Select(DefectConverter.ToDto).ToList() ?? new List<DefectDto>();

            #region [ Files ]

            destination.AnnualInspection_File = source.AnnualInspection_File;
            destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
            destination.Registration_File = source.Registration_File;
            destination.Maintenance_File = source.Maintenance_File;
            destination.Maintenance_Expiration = source.Maintenance_Expiration;

            destination.SchemeDate = source.SchemeDate;
            destination.SchemeFile = source.SchemeFile;

            #endregion [ Files ]

            #region [ BNSF ]

            destination.BnsfOwner = source.BnsfOwner;
            destination.BnsfShipper = source.BnsfShipper;
            destination.BnsfUnit = source.BnsfUnit;
            destination.BnsfLoadEmptyStatus = source.BnsfLoadEmptyStatus;
            destination.BnsfCurrentLocation = source.BnsfCurrentLocation;
            destination.BnsfEventCode = source.BnsfEventCode;
            destination.BnsfEventDate = source.BnsfEventDate;
            destination.BnsfETA_Deramp = source.BnsfETA_Deramp;
            destination.BnsfOrigin = source.BnsfOrigin;
            destination.BnsfDestination = source.BnsfDestination;
            destination.BnsfBeyondGoalHours = source.BnsfBeyondGoalHours;
            destination.BnsfTimeDeramp = source.BnsfTimeDeramp;
            destination.BnsfTimeNotify = source.BnsfTimeNotify;
            destination.BnsfFinalTrainArrival = source.BnsfFinalTrainArrival;
            destination.BnsfGoalDate = source.BnsfGoalDate;
            destination.BnsfNextevent = source.BnsfNextevent;
            destination.BnsfPickUpNumber = source.BnsfPickUpNumber;
            destination.BnsfStorageDay = source.BnsfStorageDay;
            destination.BnsfTrain = source.BnsfTrain;
            destination.BnsfTrucker = source.BnsfTrucker;
            destination.BnsfWaybillDate = source.BnsfWaybillDate;
            destination.BnsfRateAuthority = source.BnsfRateAuthority;

            if (source.BnsfAmountDue != null) {
                destination.BnsfAmountDue = ((decimal)source.BnsfAmountDue).ToString("c");
            }
            destination.BnsfDueDate = source.BnsfDueDate;
            destination.BnsfWaybillNumber = source.BnsfWaybillNumber;
            destination.BnsfBillNumber = source.BnsfBillNumber;
            destination.BnsfAdjustments = source.BnsfAdjustments;

            #endregion [ BNSF ]

            return destination;
        }
    }
}
