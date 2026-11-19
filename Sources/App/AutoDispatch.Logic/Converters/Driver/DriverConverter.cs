using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Driver/DtoToVm, Driver/VmToDto (тела ConvertCore как есть) </summary>
    public static class DriverConverter {

        public static DriverViewModel ToVm(DriverDto source) {
            if (source == null) return null;
            var destination = new DriverViewModel();
            BaseConverter.FillDispatchVm(source, destination);

            destination.LoadId = source.LoadId;
            destination.TruckId = source.TruckId;
            destination.ChatStatus = source.ChatStatus;
            destination.DriverStatus = source.DriverStatus;
            destination.Email = source.Email;
            destination.Phone = source.Phone;
            destination.Name = source.Name;

            destination.CarrierViewId = source.CarrierViewId;
            destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;

            #region [ Files ]

            destination.THE_APPLICATION_OF_EMPLOYMENT_File = source.THE_APPLICATION_OF_EMPLOYMENT_File;
            destination.MVR_File = source.MVR_File;
            destination.MEDICAL_CARD_File = source.MEDICAL_CARD_File;
            destination.CDL_File = source.CDL_File;
            destination.DOT_INSPECTIONS_File = source.DOT_INSPECTIONS_File;
            destination.BACKGROUND_File = source.BACKGROUND_File;
            destination.DRUG_TEST_File = source.DRUG_TEST_File;
            destination.RANDOM_DRUG_TEST_File = source.RANDOM_DRUG_TEST_File;
            destination.ACH_File = source.ACH_File;
            destination.W9_File = source.W9_File;

            destination.MVR_Expiration = source.MVR_Expiration;
            destination.MEDICAL_CARD_Expiration = source.MEDICAL_CARD_Expiration;
            destination.CDL_Expiration = source.CDL_Expiration;
            destination.ACH_Expiration = source.ACH_Expiration;
            destination.W9_Expiration = source.W9_Expiration;

            #endregion [ Files ]

            return destination;
        }

        public static DriverDto ToDto(DriverViewModel source) {
            if (source == null) return null;
            var destination = new DriverDto();
            BaseConverter.FillDispatchDto(source, destination);

            destination.LoadId = source.LoadId;
            destination.TruckId = source.TruckId;
            destination.ChatStatus = source.ChatStatus;
            destination.DriverStatus = source.DriverStatus;
            destination.Email = source.Email;
            destination.Phone = source.Phone;
            destination.Name = source.Name;

            destination.CarrierViewId = source.CarrierViewId;
            destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;

            #region [ Files ]

            destination.THE_APPLICATION_OF_EMPLOYMENT_File = source.THE_APPLICATION_OF_EMPLOYMENT_File;
            destination.MVR_File = source.MVR_File;
            destination.MEDICAL_CARD_File = source.MEDICAL_CARD_File;
            destination.CDL_File = source.CDL_File;
            destination.DOT_INSPECTIONS_File = source.DOT_INSPECTIONS_File;
            destination.BACKGROUND_File = source.BACKGROUND_File;
            destination.DRUG_TEST_File = source.DRUG_TEST_File;
            destination.RANDOM_DRUG_TEST_File = source.RANDOM_DRUG_TEST_File;
            destination.ACH_File = source.ACH_File;
            destination.W9_File = source.W9_File;

            destination.MVR_Expiration = source.MVR_Expiration;
            destination.MEDICAL_CARD_Expiration = source.MEDICAL_CARD_Expiration;
            destination.CDL_Expiration = source.CDL_Expiration;
            destination.ACH_Expiration = source.ACH_Expiration;
            destination.W9_Expiration = source.W9_Expiration;

            #endregion [ Files ]

            return destination;
        }
    }
}
