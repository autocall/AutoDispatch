using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 ShipperReceiverCompany/DtoToVm, ShipperReceiverCompany/VmToDto </summary>
    public static class ShipperReceiverCompanyConverter {

        public static ShipperReceiverCompanyViewModel ToVm(ShipperReceiverCompanyDto source) {
            if (source == null) return null;
            var result = new ShipperReceiverCompanyViewModel();
            BaseConverter.FillVm(source, result);

            result.Contacts = ContactsConverter.ToVm(source.Contacts);
            result.Name = source.Name;

            return result;
        }

        public static ShipperReceiverCompanyDto ToDto(ShipperReceiverCompanyViewModel source) {
            if (source == null) return null;
            var result = new ShipperReceiverCompanyDto();
            BaseConverter.FillDto(source, result);

            result.Contacts = ContactsConverter.ToDto(source.Contacts);
            result.Name = source.Name;

            return result;
        }
    }
}
