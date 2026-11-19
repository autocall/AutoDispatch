using System;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Census/CensusToDatCompany.Convert </summary>
    public static class CensusConverter {

        public static DatCompanyViewModel ToDatCompany(CensusCompanyDto dto) {
            var contacts = new ContactsViewModel();
            if (String.IsNullOrEmpty(dto.TEL_NUM) == false) {
                contacts.Contacts.Add(new ContactPhoneViewModel() { Phone = StringExtension.OnlyNumbers(dto.TEL_NUM) });
            }
            return new DatCompanyViewModel() {
                McNumber = dto.ICC_DOCKET_1_PREFIX == "MC" ? dto.ICC1 :
                           dto.ICC_DOCKET_2_PREFIX == "MC" ? dto.ICC2 :
                           dto.ICC_DOCKET_3_PREFIX == "MC" ? dto.ICC3 : null,
                DotNumber = dto.DOT_NUMBER,
                Name = dto.NAME,
                Contacts = contacts
            };
        }
    }
}
