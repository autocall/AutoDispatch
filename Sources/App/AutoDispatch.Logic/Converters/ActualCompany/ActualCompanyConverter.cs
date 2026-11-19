using Application = System.Windows.Application;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 ActualCompany/DtoToVm, ActualCompany/VmToDto (тела ConvertCore как есть) </summary>
    public static class ActualCompanyConverter {

        public static ActualCompanyViewModel ToVm(ActualCompanyDto source) {
            if (source == null) return null;
            var destination = new ActualCompanyViewModel();
            BaseConverter.FillVm(source, destination);

            destination.McNumber = source.McNumber;
            destination.BoardType = source.BoardType;
            destination.EntityType = source.EntityType;
            destination.LegalName = source.LegalName;

            destination.PhysicalAddress = source.PhysicalAddress;
            destination.MailingAddress = source.MailingAddress;

            destination.ShippersPhone = source.ShippersPhone;
            destination.CellPhone = source.CellPhone;

            destination.WorkPhone = source.WorkPhone;
            destination.WorkFax = source.WorkFax;
            destination.WorkEmail = source.WorkEmail;

            destination.SMTP_SchemeEmail = source.SMTP_SchemeEmail;

            destination.SMS_Phone = source.SMS_Phone;

            destination.ConfirmationFile = source.ConfirmationFile;
            destination.BOLFile = source.BOLFile;
            destination.InvoiceFile = source.InvoiceFile;
            destination.ConfirmationData = source.ConfirmationData;
            destination.BOLData = source.BOLData;
            destination.InvoiceData = source.InvoiceData;

            destination.PaymentInstructions = source.PaymentInstructions;

            destination.CarrierViewToken = source.CarrierViewToken;

            if (String.IsNullOrEmpty(source.FactoringCompanyJson)) {
                destination.FactoringCompany = new FactoringCompanyViewModel();
            } else {
                var dto = JsonExtension.DeserializeObject<FactoringCompanyDto>(source.FactoringCompanyJson);
                destination.FactoringCompany = FactoringCompanyConverter.ToVm(dto);
            }

            destination.BnsfLogin = source.BnsfLogin;
            destination.BnsfLastMessage = source.BnsfLastMessage;

            Application.Current.Dispatcher.Invoke((Action)(() => {
                // TODO: use StringBuilder or something similar
                if (source.LogoSmall != null && source.LogoSmall.StartsWith("data:image")) {
                    destination.LogoSmall = Base64ToImage(source.LogoSmall);
                }
                if (source.LogoBig != null && source.LogoBig.StartsWith("data:image")) {
                    destination.LogoBig = Base64ToImage(source.LogoBig);
                }
            }));

            return destination;
        }

        public static ActualCompanyDto ToDto(ActualCompanyViewModel source) {
            if (source == null) return null;
            var destination = new ActualCompanyDto();
            BaseConverter.FillDto(source, destination);

            destination.McNumber = source.McNumber;
            destination.BoardType = source.BoardType;
            destination.EntityType = source.EntityType;
            destination.LegalName = source.LegalName;

            destination.PhysicalAddress = source.PhysicalAddress;
            destination.MailingAddress = source.MailingAddress;

            destination.ShippersPhone = source.ShippersPhone;
            destination.CellPhone = source.CellPhone;

            destination.WorkPhone = source.WorkPhone;
            destination.WorkFax = source.WorkFax;
            destination.WorkEmail = source.WorkEmail;
           
            destination.SMS_Phone = source.SMS_Phone;

            destination.ConfirmationFile = source.ConfirmationFile;
            destination.BOLFile = source.BOLFile;
            destination.InvoiceFile = source.InvoiceFile;
            destination.ConfirmationData = source.ConfirmationData;
            destination.BOLData = source.BOLData;
            destination.InvoiceData = source.InvoiceData;

            destination.PaymentInstructions = source.PaymentInstructions;

            destination.CarrierViewToken = source.CarrierViewToken;

            var dto = FactoringCompanyConverter.ToDto(source.FactoringCompany);
            destination.FactoringCompanyJson = JsonExtension.SerializeObject(dto);

            destination.BnsfLastMessage = source.BnsfLastMessage;

            // NOTE: skip logo icons

            return destination;
        }

        private static BitmapImage Base64ToImage(string source) {
            source = new Regex(@"^data:image\/\w+;base64,").Replace(source, "");
            byte[] binaryData = System.Convert.FromBase64String(source);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(binaryData);
            bi.EndInit();

            return bi;
        }
    }
}
