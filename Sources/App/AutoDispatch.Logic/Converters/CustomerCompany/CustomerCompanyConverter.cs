using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 CustomerCompany/DtoToVm, CustomerCompany/VmToDto </summary>
    public static class CustomerCompanyConverter {

        public static CustomerCompanyViewModel ToVm(CustomerCompanyDto source) {
            if (source == null) return null;
            var result = new CustomerCompanyViewModel();
            BaseConverter.FillVm(source, result);

            result.Contacts = ContactsConverter.ToVm(source.Contacts);
            result.Name = source.Name;

            return result;
        }

        public static CustomerCompanyDto ToDto(CustomerCompanyViewModel source) {
            if (source == null) return null;
            var result = new CustomerCompanyDto();
            BaseConverter.FillDto(source, result);

            result.Contacts = ContactsConverter.ToDto(source.Contacts);
            result.Name = source.Name;

            return result;
        }
    }
}
