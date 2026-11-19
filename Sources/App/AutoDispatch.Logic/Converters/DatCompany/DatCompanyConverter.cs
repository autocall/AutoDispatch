using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 DatCompany/DtoToVm, DatCompany/VmToDto (тела ConvertCore как есть) </summary>
    public static class DatCompanyConverter {

        public static DatCompanyViewModel ToVm(DatCompanyDto source) {
            if (source == null) return null;
            var result = new DatCompanyViewModel();
            BaseConverter.FillEventVm(source, result);

            result.Contacts = ContactsConverter.ToVm(source.Contacts);
            result.ActualCompanyId = source.ActualCompanyId;
            result.EntityType = source.EntityType;
            result.InsuranceExpiration = source.InsuranceExpiration;
            result.InsuranceFilePath = source.InsuranceFilePath;
            result.InsuranceFileData = source.InsuranceFileData;
            result.McNumber = source.McNumber;
            result.DotNumber = source.DotNumber;
            result.Name = source.Name;
            result.Phy_UpsStore = source.Phy_UpsStore;
            result.Mai_UpsStore = source.Mai_UpsStore;
            result.Phy_MailBox = source.Phy_MailBox;
            result.Mai_MailBox = source.Mai_MailBox;
            result.Comments = source.Comments;
            result.Rating = source.Rating;
            result.Fields = source.Fields;
            result.HyperlinkFile = source.HyperlinkFile;

            result.SetupPacketId = source.SetupPacketId;
            result.SetupPacketEmailSent = source.SetupPacketEmailSent;
            result.SetupPacketSent = source.SetupPacketSent;
            result.SetupPacketCompleted = source.SetupPacketCompleted;
            result.SetupPacketAccept = source.SetupPacketAccept;
            result.SetupPacketFile = source.SetupPacketFile;
            result.W9File = source.W9File;
            result.DoNotUse = source.DoNotUse;

            return result;
        }

        public static DatCompanyDto ToDto(DatCompanyViewModel source) {
            if (source == null) return null;
            var result = new DatCompanyDto();
            BaseConverter.FillEventDto(source, result);

            result.Contacts = ContactsConverter.ToDto(source.Contacts);
            result.ActualCompanyId = source.ActualCompanyId;
            result.EntityType = source.EntityType;
            result.InsuranceExpiration = source.InsuranceExpiration;
            result.InsuranceFilePath = source.InsuranceFilePath;
            result.InsuranceFileData = source.InsuranceFileData;
            result.McNumber = source.McNumber;
            result.DotNumber = source.DotNumber;
            result.Name = source.Name;
            result.Phy_UpsStore = source.Phy_UpsStore;
            result.Mai_UpsStore = source.Mai_UpsStore;
            result.Phy_MailBox = source.Phy_MailBox;
            result.Mai_MailBox = source.Mai_MailBox;
            result.Comments = source.Comments;
            result.Rating = source.Rating;
            result.Fields = source.Fields;
            result.HyperlinkFile = source.HyperlinkFile;

            result.SetupPacketId = source.SetupPacketId;
            result.SetupPacketEmailSent = source.SetupPacketEmailSent;
            result.SetupPacketSent = source.SetupPacketSent;
            result.SetupPacketCompleted = source.SetupPacketCompleted;
            result.SetupPacketAccept = source.SetupPacketAccept;
            result.SetupPacketFile = source.SetupPacketFile;
            result.W9File = source.W9File;
            result.DoNotUse = source.DoNotUse;

            return result;
        }
    }
}
