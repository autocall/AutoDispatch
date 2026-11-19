using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Факторинговая компания (JSON ActualCompanyDto.FactoringCompanyJson). В V1 — автоматическая пара AutoMapper
    ///     FactoringCompanyDto ↔ FactoringCompanyViewModel (ReverseMap), вызывалась из конвертера ActualCompany и UI </summary>
    public static class FactoringCompanyConverter {

        public static FactoringCompanyViewModel ToVm(FactoringCompanyDto source) {
            if (source == null) return null;
            return new FactoringCompanyViewModel() {
                HasFactoringCompany = source.HasFactoringCompany,
                CompanyName = source.CompanyName,
                Address = source.Address,
                City = source.City,
                Phone = source.Phone,
                Email = source.Email,
            };
        }

        public static FactoringCompanyDto ToDto(FactoringCompanyViewModel source) {
            if (source == null) return null;
            return new FactoringCompanyDto() {
                HasFactoringCompany = source.HasFactoringCompany,
                CompanyName = source.CompanyName,
                Address = source.Address,
                City = source.City,
                Phone = source.Phone,
                Email = source.Email,
            };
        }
    }
}
