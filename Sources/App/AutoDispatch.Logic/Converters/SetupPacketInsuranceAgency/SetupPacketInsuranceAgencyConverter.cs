using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Страховое агентство setup packet. В V1 — автоматическая пара AutoMapper
    ///     SetupPacketInsuranceAgencyDto ↔ SetupPacketInsuranceAgencyViewModel (ReverseMap), вызывалась из конвертера SetupPacket и UI </summary>
    public static class SetupPacketInsuranceAgencyConverter {

        public static SetupPacketInsuranceAgencyViewModel ToVm(SetupPacketInsuranceAgencyDto source) {
            if (source == null) return null;
            return new SetupPacketInsuranceAgencyViewModel() {
                EntityId = source.EntityId,
                HasInsurance = source.HasInsurance,
                CompanyName = source.CompanyName,
                Phone = source.Phone,
                Fax = source.Fax,
                Email = source.Email,
                Number = source.Number,
            };
        }

        public static SetupPacketInsuranceAgencyDto ToDto(SetupPacketInsuranceAgencyViewModel source) {
            if (source == null) return null;
            return new SetupPacketInsuranceAgencyDto() {
                EntityId = source.EntityId,
                HasInsurance = source.HasInsurance,
                CompanyName = source.CompanyName,
                Phone = source.Phone,
                Fax = source.Fax,
                Email = source.Email,
                Number = source.Number,
            };
        }
    }
}
