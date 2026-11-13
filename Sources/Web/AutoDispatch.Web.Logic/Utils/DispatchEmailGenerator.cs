using System.Text;
using AutoMapper;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Email;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Utils;

/// <summary>
///     Письма компаний по шаблонам Resources/Emails/*.html (V1 DispatchEmailGenerator). Ссылки на сайт — AppSettings.Website
///     вместо HttpContext.Current.Request.Url: страницы /Confirmation и /CarrierRegistration — этап 5 </summary>
public static class DispatchEmailGenerator {

    private static string EmailTemplate(string name) => Path.Combine(AppSettings.EmailsPath, name);

    private static string FilePath(LoadDto load, string fileName) => DispatchFileHelper.FileGetFullPath(load.EntityId, fileName);

    /// <summary> </summary>
    /// <param name="children"> Using for multi-stops</param>
    private static string _replaceFields1(IMapper mapper, StringBuilder sb, DatCompanyDto carrier, LoadDto load, List<LoadDto> children, ActualCompanyDto company, bool showPickupAddress = true) {

        #region [ init fields ]

        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[big_logo]", company.LogoBig);
        companyFields.Add("[now]", DateTime.Now.ToShortDateString());
        companyFields.Add("[equipment]", AutoCall.Common.Extensions.EnumExtensions.GetDescriptionOrTostring((AutoCallEquipmentType)load.Equipment));
        if (carrier != null) {
            companyFields.Add("[carrier]", carrier.Name);
            companyFields.Add("[carrier_mc]", carrier.McNumber.ToString());
            if (carrier.Fields != null && carrier.Fields.ContainsKey("Phone:")) {
                companyFields.Add("[carrier_phone]", "Phone: " + carrier.Fields["Phone:"]);
            } else {
                companyFields.Add("[carrier_phone]", "");
            }
        }
        companyFields.Add("[host_confirmation]", $"{AppSettings.Website}/Confirmation/?LoadId={load.EntityId}");

        var pickupsFields = new List<Dictionary<string, string>>();
        var deliveriesFields = new List<Dictionary<string, string>>();
        var loadPickups = children.Any() ? children : new List<LoadDto>() { load };
        foreach (LoadDto l in loadPickups) {
            var originCity = showPickupAddress ? $"{l.OriginAddress}, {l.OriginCity}" : l.OriginCity;
            // имеется такой же адресс, значит надо использовать root
            var pickupFields = pickupsFields.FirstOrDefault(e => e["[origin]"] == originCity);
            if (pickupFields == null) {
                var list = new Dictionary<string, string>();
                list.Add("[load_id]", l.IndexString);
                list.Add("[pickup_date]", DispatchStringExtension.DateTimeToAppointmentFullDateTime(l.PickupFrom.Date, l.PickupFrom.TimeOfDay, l.PickupTo.TimeOfDay));
                list.Add("[origin]", originCity);
                list.Add("[shipper]", showPickupAddress ? l.ShipperCompany : $"Driver must Call {company.WorkPhone} for Dispatch");
                list.Add("[weight]", NullableWeightToString(l.Weight, (WeightType)l.WeightType));
                list.Add("[length]", l.Length.NullableLengthToString());
                list.Add("[pallets]", l.Pallets.NullableToString());
                pickupsFields.Add(list);
            } else {
                pickupFields["[load_id]"] = load.IndexString;
                pickupFields["[pickup_date]"] = DispatchStringExtension.DateTimeToAppointmentFullDateTime(load.PickupFrom.Date, load.PickupFrom.TimeOfDay, load.PickupTo.TimeOfDay);
                pickupFields["[origin]"] = originCity;
                pickupFields["[shipper]"] = showPickupAddress ? load.ShipperCompany : $"Driver must Call {company.WorkPhone} for Dispatch";
                pickupFields["[weight]"] = NullableWeightToString(load.Weight, (WeightType)load.WeightType);
                pickupFields["[length]"] = load.Length.NullableLengthToString();
                pickupFields["[pallets]"] = load.Pallets.NullableToString();
            }
        }

        var loadDeliveries = children.Any() ? children : new List<LoadDto>() { load };
        foreach (var l in loadDeliveries) {
            var destinationCity = $"{l.DestinationAddress}, {l.DestinationCity}";
            var deliveryFields = deliveriesFields.FirstOrDefault(e => e["[destination]"] == destinationCity);
            if (deliveryFields == null) {
                var list = new Dictionary<string, string>();
                list.Add("[load_id]", l.IndexString);
                list.Add("[delivery_date]", DispatchStringExtension.DateTimeToAppointmentFullDateTime(l.DeliveryFrom.Date, l.DeliveryFrom.TimeOfDay, l.DeliveryTo.TimeOfDay));
                list.Add("[destination]", destinationCity);
                list.Add("[receiver]", l.ReceiverCompany);
                list.Add("[weight]", NullableWeightToString(l.Weight, (WeightType)l.WeightType));
                list.Add("[length]", l.Length.NullableLengthToString());
                list.Add("[pallets]", l.Pallets.NullableToString());
                deliveriesFields.Add(list);
            } else {
                deliveryFields["[load_id]"] = load.IndexString;
                deliveryFields["[delivery_date]"] = DispatchStringExtension.DateTimeToAppointmentFullDateTime(load.DeliveryFrom.Date, load.DeliveryFrom.TimeOfDay, load.DeliveryTo.TimeOfDay);
                deliveryFields["[destination]"] = destinationCity;
                deliveryFields["[receiver]"] = load.ReceiverCompany;
                deliveryFields["[weight]"] = NullableWeightToString(load.Weight, (WeightType)load.WeightType);
                deliveryFields["[length]"] = load.Length.NullableLengthToString();
                deliveryFields["[pallets]"] = load.Pallets.NullableToString();
            }
        }

        companyFields.Add("[rate]", DispatchHelper.GetRate(load).NullableToString("N2"));

        #endregion [ init fields ]

        #region [ rate ]

        {
            var beginIndex = IndexOf(sb, "[begin_rate_template]");
            sb.Replace("[begin_rate_template]", "");
            var endIndex = IndexOf(sb, "[end_rate_template]");
            sb.Replace("[end_rate_template]", "");
            char[] rateTemplate = new char[endIndex - beginIndex];
            sb.CopyTo(beginIndex, rateTemplate, 0, rateTemplate.Length);
            sb.Remove(beginIndex, rateTemplate.Length);

            var currency = (CurrencyType)load.Currency;
            var rates = new List<Tuple<string, string>>();
            if (load.CarrierViewTracking > 0) {
                rates.Add(new Tuple<string, string>("CarrierView Tracking:", $"{currency.GetDescription()}{load.CarrierViewTracking.Value:N2}"));
            }

            foreach (var gain in mapper.Map<List<GainViewModel>>(load.Gains)) {
                if (gain.Cost != null) {
                    rates.Add(new Tuple<string, string>($"{gain.TypeDescription()}:", $"{currency.GetDescription()}{gain.Cost:N2}"));
                }
            }
            if (rates.Any()) {
                rates.Insert(0, new Tuple<string, string>($"Line Haul:", $"{currency.GetDescription()}{load.LineHaul:N2}"));
            }

            foreach (Tuple<string, string> field in rates) {
                var template = new StringBuilder(new String(rateTemplate));
                template.Replace("[rate_label]", field.Item1);
                template.Replace("[rate_value]", field.Item2);
                sb.Insert(beginIndex, template.ToString());
            }
        }

        #endregion [ rate ]

        #region [ replace fields ]

        {
            var beginIndex = IndexOf(sb, "[begin_pickup_template]");
            sb.Replace("[begin_pickup_template]", "");
            var endIndex = IndexOf(sb, "[end_pickup_template]");
            sb.Replace("[end_pickup_template]", "");
            char[] pickupTemplate = new char[endIndex - beginIndex];
            sb.CopyTo(beginIndex, pickupTemplate, 0, pickupTemplate.Length);
            sb.Remove(beginIndex, pickupTemplate.Length);

            beginIndex = IndexOf(sb, "[begin_delivery_template]");
            sb.Replace("[begin_delivery_template]", "");
            endIndex = IndexOf(sb, "[end_delivery_template]");
            sb.Replace("[end_delivery_template]", "");
            char[] deliveryTemplate = new char[endIndex - beginIndex];
            sb.CopyTo(beginIndex, deliveryTemplate, 0, deliveryTemplate.Length);
            sb.Remove(beginIndex, deliveryTemplate.Length);

            pickupsFields.Reverse();
            deliveriesFields.Reverse();

            foreach (Dictionary<string, string> loadFields in deliveriesFields) {
                var template = new StringBuilder(new String(deliveryTemplate));
                foreach (var field in loadFields) {
                    template.Replace(field.Key, field.Value);
                }
                sb.Insert(beginIndex, template.ToString());
            }
            foreach (Dictionary<string, string> loadFields in pickupsFields) {
                var template = new StringBuilder(new String(pickupTemplate));
                foreach (var field in loadFields) {
                    template.Replace(field.Key, field.Value);
                }
                sb.Insert(beginIndex, template.ToString());
            }

            foreach (var field in companyFields) {
                sb.Replace(field.Key, field.Value);
            }
        }

        #endregion [ replace fields ]

        return sb.ToString();
    }

    private static string _replaceFields2(StringBuilder sb, DatCompanyDto carrier, LoadDto load, ActualCompanyDto company) {
        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[now]", DateTime.Now.ToShortDateString());
        companyFields.Add("[carrier]", carrier.Name);
        companyFields.Add("[carrier_mc]", carrier.McNumber.ToString());
        companyFields.Add("[host_confirmed]", $"{AppSettings.Website}/Confirmation/Download/{load.EntityId}");
        foreach (var field in companyFields) {
            sb.Replace(field.Key, field.Value);
        }
        return sb.ToString();
    }

    private static string _replaceFields3(StringBuilder sb, DatCompanyDto carrier, ActualCompanyDto company) {
        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[big_logo]", company.LogoBig);
        companyFields.Add("[now]", DateTime.Now.ToShortDateString());
        companyFields.Add("[carrier]", carrier.Name);
        companyFields.Add("[carrier_mc]", carrier.McNumber?.ToString() ?? "-");
        companyFields.Add("[host_setup_packet]", $"{AppSettings.Website}/CarrierRegistration/?CarrierId={carrier.EntityId}");
        foreach (var field in companyFields) {
            sb.Replace(field.Key, field.Value);
        }
        return sb.ToString();
    }

    private static string _replaceFields4(StringBuilder sb, DatCompanyDto carrier, ActualCompanyDto company) {
        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[big_logo]", company.LogoBig);
        companyFields.Add("[company_name]", company.LegalName);
        foreach (var field in companyFields) {
            sb.Replace(field.Key, field.Value);
        }
        return sb.ToString();
    }

    private static string _replaceFields6(StringBuilder sb, string trailer, ActualCompanyDto company) {
        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[trailer]", trailer);
        companyFields.Add("[big_logo]", company.LogoBig);
        foreach (var field in companyFields) {
            sb.Replace(field.Key, field.Value);
        }
        return sb.ToString();
    }

    private static string _replaceFields7(StringBuilder sb, ActualCompanyDto company) {
        var companyFields = new Dictionary<string, string>();
        companyFields.Add("[ActualCompany.LegalName]", company.LegalName);
        companyFields.Add("[ActualCompany.PhysicalAddress1]", company.PhysicalAddress.Trim().Split('\n').First());
        companyFields.Add("[ActualCompany.PhysicalAddress2]", company.PhysicalAddress.Trim().Split('\n').Last());
        foreach (var field in companyFields) {
            sb.Replace(field.Key, field.Value);
        }
        return sb.ToString();
    }

    public static async Task<Email.Email> GenerateConfirmationAsync(IMapper mapper, DatCompanyDto carrier, LoadDto load, List<LoadDto> children, ActualCompanyDto company, List<string> toEmails) {
        if (company.SMTP_Email == null) {
            throw new Exception("SMTP_Email is null");
        }
        if (load.ConfirmationFile == null) {
            _l.e($"Load '{load.EntityId}' ConfirmationFile is null");
            throw new Exception("ConfirmationFile is null");
        }
        var content = _replaceFields1(mapper, new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("confirmation.html"))), carrier, load, children, company, load.ShowPickupAddress);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = $"{load.OriginCity} → {load.DestinationCity}";
        email.Subject = $"Confirmation #{load.IndexString}";
        email.ToEmailAddresses = toEmails.Select(e => new EmailAddress(e)).ToList();
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        // NOTES: don't send file of unsigned confirmation
        InsertFooter(email, company);
        return email;
    }

    public static async Task<Email.Email> GenerateInvoiceAsync(DatCompanyDto datBroker, LoadDto load, ActualCompanyDto company, List<string> emails,
        bool sendInvoice, bool sendPOD, bool sendConfirmation, bool sendLumper, bool merged, string text) {
        if (company.SMTP_Email == null) {
            throw new Exception("SMTP_Email is null");
        }

        var content = await File.ReadAllTextAsync(EmailTemplate("invoice.html"));
        content = content.Replace("[text]", (text ?? "").Replace("\n", "<br/>"));

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = $"{load.OriginCity} → {load.DestinationCity}";
        email.Subject = $"Invoice #{load.InvoiceNumber} Load #{load.RecordId}";
        email.ToEmailAddresses = emails.Select(e => new EmailAddress(e)).ToList();
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, "ACCOUNTING");
        email.Attachments = new List<EmailAttachment>();

        if (merged == false) {
            if (sendInvoice && String.IsNullOrEmpty(load.InvoiceFile) == false) {
                var attach_path = FilePath(load, load.InvoiceFile);
                if (File.Exists(attach_path))
                    email.Attachments.Add(new EmailAttachment() {
                        FileName = $"Invoice #{load.InvoiceNumber}{Path.GetExtension(load.InvoiceFile)}",
                        Content = Convert.ToBase64String(File.ReadAllBytes(attach_path))
                    });
            }
            if (sendPOD && String.IsNullOrEmpty(load.PODFile ?? load.BOLFile) == false) {
                var attach_path = FilePath(load, load.PODFile ?? load.BOLFile);
                if (File.Exists(attach_path))
                    email.Attachments.Add(new EmailAttachment() {
                        FileName = $"POD #{DispatchStringExtension.ToFileName(load.RecordId)}{Path.GetExtension(load.PODFile ?? load.BOLFile)}",
                        Content = Convert.ToBase64String(File.ReadAllBytes(attach_path))
                    });
            }
            if (sendConfirmation && String.IsNullOrEmpty(load.ConfirmedFile ?? load.ConfirmationFile) == false) {
                var attach_path = FilePath(load, load.ConfirmedFile ?? load.ConfirmationFile);
                if (File.Exists(attach_path))
                    email.Attachments.Add(new EmailAttachment() {
                        FileName = $"Confirmation #{DispatchStringExtension.ToFileName(load.RecordId)}{Path.GetExtension(load.ConfirmedFile ?? load.ConfirmationFile)}",
                        Content = Convert.ToBase64String(File.ReadAllBytes(attach_path))
                    });
            }
            if (sendLumper && String.IsNullOrEmpty(load.LumperFile) == false) {
                var attach_path = FilePath(load, load.LumperFile);
                if (File.Exists(attach_path))
                    email.Attachments.Add(new EmailAttachment() {
                        FileName = $"Lumper #{DispatchStringExtension.ToFileName(load.RecordId)}{Path.GetExtension(load.LumperFile)}",
                        Content = Convert.ToBase64String(File.ReadAllBytes(attach_path))
                    });
            }
        } else {
            var file = GenerateMergedInvoice(load, sendInvoice, sendPOD, sendConfirmation, sendLumper);
            if (file != null) {
                email.Attachments.Add(new EmailAttachment() {
                    FileName = file.Name,
                    Content = Convert.ToBase64String(file.Data)
                });
            }
        }

        InsertFooter(email, company);
        return email;
    }

    public static FileDto GenerateMergedInvoice(LoadDto load, bool sendInvoice, bool sendPOD, bool sendConfirmation, bool sendLumper) {
        var files = new List<string>();
        if (sendInvoice && String.IsNullOrEmpty(load.InvoiceFile) == false && load.InvoiceFile.EndsWith(".pdf")) {
            var attach_path = FilePath(load, load.InvoiceFile);
            if (File.Exists(attach_path)) files.Add(attach_path);
        }
        if (sendPOD && String.IsNullOrEmpty(load.PODFile ?? load.BOLFile) == false && (load.PODFile ?? load.BOLFile).EndsWith(".pdf")) {
            var attach_path = FilePath(load, load.PODFile ?? load.BOLFile);
            if (File.Exists(attach_path)) files.Add(attach_path);
        }
        if (sendConfirmation && String.IsNullOrEmpty(load.ConfirmedFile ?? load.ConfirmationFile) == false && (load.ConfirmedFile ?? load.ConfirmationFile).EndsWith(".pdf")) {
            var attach_path = FilePath(load, load.ConfirmedFile ?? load.ConfirmationFile);
            if (File.Exists(attach_path)) files.Add(attach_path);
        }
        if (sendLumper && String.IsNullOrEmpty(load.LumperFile) == false && load.LumperFile.EndsWith(".pdf")) {
            var attach_path = FilePath(load, load.LumperFile);
            if (File.Exists(attach_path)) files.Add(attach_path);
        }

        if (files.Any()) {
            var data = DispatchPdfGenerator.MergeDocs(files);
            return new FileDto() {
                Data = data,
                Name = $"Billing Invoice #{load.InvoiceNumber}.pdf",
                Size = data.Length
            };
        }
        return null;
    }

    public static async Task<Email.Email> GenerateConfirmedAsync(DatCompanyDto carrier, LoadDto load, ActualCompanyDto company, List<string> toEmails) {
        var attach_path = FilePath(load, load.ConfirmedFile);
        var content = _replaceFields2(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("confirmed.html"))), carrier, load, company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = $"{load.OriginCity} → {load.DestinationCity}";
        email.Subject = $"Confirmation #{load.IndexString}";
        email.ToEmailAddresses = toEmails.Select(e => new EmailAddress(e)).ToList();
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        email.Attachments = new List<EmailAttachment>() {
            new EmailAttachment() {
                FileName = load.ConfirmedFile,
                Content = Convert.ToBase64String(File.ReadAllBytes(attach_path))
            } };
        InsertFooter(email, company);
        return email;
    }

    private static readonly Random _rand = new Random();

    public static void InsertFooter(Email.Email email, ActualCompanyDto company) {
        var image = new EmailInlineImage();
        image.Content = company.LogoBig;
        image.ContentId = _rand.Next(100000, 9999999).ToString();
        image.FileName = "big_logo.png";

        var address = company.PhysicalAddress.Trim().Split('\n').First().Trim();
        var city = company.PhysicalAddress.Trim().Split('\n').Last().Trim();
        var footer = $@"<hr style=""border-top: 1px solid #D0D0D0;border-bottom: 0;""/><img height=""32"" src =""cid:{image.FileName}@{image.ContentId}""/><br><span>{address}</span><br><span>{city}</span>";
        if (String.IsNullOrEmpty(company.WorkPhone) == false) {
            footer += $"<br><span>{company.WorkPhone}</span>";
        }
        email.Body = email.Body.Replace("[footer]", $"<div style=\"color:gray;font-size:80%\">{footer}</div>");
    }

    public static async Task<Email.Email> GenerateSetupPacketCompletedAsync(SetupPacketDto setupPacketDto, DatCompanyDto carrier, ActualCompanyDto company, params EmailAttachment[] attachment) {
        var content = _replaceFields4(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("setup_packet_completed.html"))), carrier, company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = "";
        email.Subject = $"{company.LegalName} - Carrier Registration Confirmation";
        email.ToEmailAddresses = new List<EmailAddress>() { new EmailAddress(setupPacketDto.ConfidantContact.Email) };
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        email.Attachments = attachment.ToList();
        InsertFooter(email, company);
        return email;
    }

    public static async Task<Email.Email> GenerateInsuranceInstructionsCompletedAsync(SetupPacketDto setupPacketDto, DatCompanyDto carrier, ActualCompanyDto company, params EmailAttachment[] attachment) {
        var content = _replaceFields7(
            new StringBuilder(_replaceFields4(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("insurance_instrunctions_completed.html"))), carrier, company)),
            company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = "";
        email.Subject = $"{company.LegalName} - Insurance Instructions";
        email.ToEmailAddresses = new List<EmailAddress>() { new EmailAddress(setupPacketDto.ConfidantContact.Email) };
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        email.Attachments = attachment.ToList();
        InsertFooter(email, company);
        return email;
    }

    public static async Task<Email.Email> GenerateSetupPacketAsync(DatCompanyDto carrier, ActualCompanyDto company, List<string> toEmails) {
        var content = _replaceFields3(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("setup_packet.html"))), carrier, company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = "";
        email.Subject = $"{company.LegalName} - Carrier Registration";
        email.ToEmailAddresses = toEmails.Select(e => new EmailAddress(e)).ToList();
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        InsertFooter(email, company);
        return email;
    }

    public static async Task<Email.Email> GenerateEditSetupPacketAsync(DatCompanyDto carrier, ActualCompanyDto company, List<string> toEmails) {
        var content = _replaceFields3(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("setup_packet_edit.html"))), carrier, company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = "";
        email.Subject = $"{company.LegalName} - Carrier Registration";
        email.ToEmailAddresses = toEmails.Select(e => new EmailAddress(e)).ToList();
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        InsertFooter(email, company);
        return email;
    }

    public static async Task<Email.Email> GenerateSchemeEmailAsync(string emailAddress, string trailerNumber, byte[] data, ActualCompanyDto company) {
        var content = _replaceFields6(new StringBuilder(await File.ReadAllTextAsync(EmailTemplate("scheme.html"))), trailerNumber, company);

        var email = new Email.Email();
        email.IsBodyHTML = true;
        email.Body = content;
        email.Description = "";
        email.Subject = trailerNumber;
        email.ToEmailAddresses = new List<EmailAddress>() { new EmailAddress(emailAddress) };
        email.FromEmailAddress = new EmailAddress(company.SMTP_Email, company.LegalName);
        email.Attachments.Add(new EmailAttachment() {
            FileName = $"{trailerNumber.OnlyNumbersAndLetters()}.pdf",
            Content = Convert.ToBase64String(data)
        });
        InsertFooter(email, company);
        return email;
    }

    public static string NullableWeightToString(int? value, WeightType type) {
        if (value.HasValue && value != 0) return $"{value} {type.GetDescription()}";
        return "";
    }

    private static int IndexOf(StringBuilder sb, string value) {
        return sb.ToString().IndexOf(value, StringComparison.Ordinal);
    }
}
