using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Truck/DtoToVm, Truck/VmToDto (тела ConvertCore как есть) </summary>
    public static class TruckConverter {

        public static TruckViewModel ToVm(TruckDto source) {
            if (source == null) return null;
            var destination = new TruckViewModel();
            BaseConverter.FillDispatchVm(source, destination);

            destination.Number = source.Number;
            destination.TrailerId = source.TrailerId;

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
            destination.Registration_Expiration = source.Registration_Expiration;

            #endregion [ Files ]

            return destination;
        }

        public static TruckDto ToDto(TruckViewModel source) {
            if (source == null) return null;
            var destination = new TruckDto();
            BaseConverter.FillDispatchDto(source, destination);

            destination.Number = source.Number;
            destination.TrailerId = source.TrailerId;

            destination.Defects = source.Defects?.Select(DefectConverter.ToDto).ToList() ?? new List<DefectDto>();

            #region [ Files ]

            destination.AnnualInspection_File = source.AnnualInspection_File;
            destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
            destination.Registration_File = source.Registration_File;
            destination.Registration_Expiration = source.Registration_Expiration;

            #endregion [ Files ]

            return destination;
        }
    }
}
