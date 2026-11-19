using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Helpers;
using AutoDispatch.Data;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using Telerik.Windows.Documents.FormatProviders.Rtf;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.TextSearch;
using Table = Telerik.Windows.Documents.Model.Table;
using TableRow = Telerik.Windows.Documents.Model.TableRow;

namespace AutoDispatch.Logic.Services {
    public class ReportService {

        public ReportService() {
        }

        /// <summary>
        ///     Gets escaped Name </summary>
        public string ToFileName(string name) {
            return new String(name.Trim().Where(ch => Char.IsDigit(ch) || Char.IsLetter(ch) || ch == ' ').ToArray()).Replace(" ", "_").ToLower();
        }

        /// <summary>
        ///     Generates Confirmation document. Document without condifration#, that will be set later </summary>
        /// <param name="company">Carrier View Model</param>
        /// <param name="driver">Driver View Model</param>
        /// <param name="actualCompany">Selected Actual Company View Model on MainVindow</param>
        /// <param name="companyDetails"></param>
        /// <param name="loads">Load VM</param>
        /// <param name="children">Using for multi-stops</param>
        /// <returns>RadDocument</returns>
        public RadDocument GenerateConfirmation(DatCompanyViewModel company, ActualCompanyViewModel actualCompany,
            Dictionary<string, string> companyDetails, LoadViewModel load, List<LoadViewModel> children) {

            #region [ Init fields ]

            var session = UserSession.Current;
            var companyFields = new Dictionary<string, string>();
            companyFields.Add("[load_ids]", load.IndexString);
            companyFields.Add("[now]", DateTime.Now.ToShortDateString());
            var eqStr = load.Equipment.GetDescription() ?? load.Equipment.ToString();
            if (load.Equipment == AutoCallEquipmentType.Reefer && load.Temperature.HasValue) {
                //eqStr += $" {load.Temperature.Value.ToString("+#;-#;0")}°F continue"; // ascii format
                eqStr += $" {load.Temperature.Value.ToString("+#;-#;0")}\\'b0F continue"; // rtf format
            }
            companyFields.Add("[equipment]", eqStr);
            companyFields.Add("[company_name]", company.Name);
            companyFields.Add("[company_mc]", company.McNumber.ToString());

            companyFields.Add("[shippers_phone]", actualCompany.ShippersPhone);
            companyFields.Add("[cell_phone]", actualCompany.CellPhone);

            companyFields.Add("[work_phone]", actualCompany.WorkPhone);
            companyFields.Add("[work_fax]", actualCompany.WorkFax);
            companyFields.Add("[work_email]", actualCompany.WorkEmail);

            companyFields.Add("[carrier_contact]", "");
            companyFields.Add("[agent_contact]", actualCompany.WorkPhone);
            companyFields.Add("[agent_name]", session.Name ?? "");

            companyFields.Add("[billing_address]", actualCompany.MailingAddress);
            companyFields.Add("[billing_phone]", actualCompany.WorkPhone);

            companyFields.Add("[payment_instructions]", actualCompany.PaymentInstructions != null ? String.Join(@"\par", actualCompany.PaymentInstructions.Split('\n').Select(e => @"\bullet  " + e)) : "");

            var comments = new List<RadDocument>();

            var rates = new List<Tuple<string, string>>();
            if (load.CarrierViewTracking > 0) {
                rates.Add(new Tuple<string, string>("CarrierView Tracking:", $"{load.Currency.GetDescription()}{load.CarrierViewTracking.Value:N2}"));
            }

            foreach (var gain in load.Gains) {
                if (gain.Cost != null) {
                    rates.Add(new Tuple<string, string>($"{gain.TypeDescription()}:", $"{load.Currency.GetDescription()}{gain.Cost:N2}"));
                }
            }
            if (rates.Any()) {
                rates.Insert(0, new Tuple<string, string>($"Line Haul:", $"{load.Currency.GetDescription()}{load.LineHaul:N2}"));
            }

            companyFields.Add("[accessorial_label]", String.Join(@"\par ", rates.Select(e => e.Item1)));
            companyFields.Add("[accessorial_value]", String.Join(@"\par ", rates.Select(e => e.Item2)));

            companyFields.Add("[load_line_haul]", $"{load.Currency.GetDescription()}{load.LineHaul:N2}");
            companyFields.Add("[load_rate]", $"{load.Currency.GetDescription()}{load.Rate:N2}");

            // adds comments as documetns
            var providerXaml = new XamlFormatProvider();
            if (String.IsNullOrEmpty(load.Comments) == false) {
                try {
                    var docXaml = providerXaml.Import(load.Comments);
                    if (docXaml.IsEmpty == false) {
                        comments.Add(docXaml);
                    }
                } catch {
                }
            }

            var pickupsFields = new List<Dictionary<string, string>>();
            var deliveriesFields = new List<Dictionary<string, string>>();
            var loadPickups = children.Any() ? children : new List<LoadViewModel>() { load };
            foreach (LoadViewModel l in loadPickups) {
                var pickupFields = pickupsFields.FirstOrDefault(e => e["[origin_city]"] == l.OriginCity &&
                                                                     e["[pickup_date]"] == l.PickupFullDateTime);
                if (pickupFields == null) {
                    var list = new Dictionary<string, string>();
                    list.Add("[load_id]", l.IndexString);
                    list.Add("[pickup_id]", l.PickupNumber);
                    list.Add("[pickup_date]", l.PickupFullDateTime);
                    list.Add("[origin_address]", l.ShowPickupAddress ? l.OriginAddress : "");
                    list.Add("[origin_city]", l.OriginCity);
                    list.Add("[weight]", NullableWeightToString(l.Weight, l.WeightType));
                    list.Add("[length]", l.Length.NullableLengthToString());
                    list.Add("[pallets]", l.Pallets.NullableToString());
                    list.Add("[shipper_phone]", l.ShowPickupAddress ? actualCompany.ShippersPhone ?? l.ShipperContacts.ToString() : "");
                    list.Add("[shipper]", l.ShowPickupAddress ? l.ShipperCompany : $"Driver must Call {actualCompany.WorkPhone} for Dispatch");
                    pickupsFields.Add(list);
                } else if (pickupFields["[origin_city]"] == load.OriginCity &&
                           pickupFields["[pickup_date]"] == load.PickupFullDateTime) {
                    pickupFields["[load_id]"] = load.IndexString;
                    pickupFields["[pickup_id]"] = load.PickupNumber;
                    pickupFields["[pickup_date]"] = load.PickupFullDateTime;
                    pickupFields["[origin_address]"] = load.ShowPickupAddress ? load.OriginAddress : "";
                    pickupFields["[origin_city]"] = load.OriginCity;
                    pickupFields["[weight]"] = NullableWeightToString(load.Weight, load.WeightType);
                    pickupFields["[length]"] = load.Length.NullableLengthToString();
                    pickupFields["[pallets]"] = load.Pallets.NullableToString();
                    pickupFields["[shipper_phone]"] = load.ShowPickupAddress ? actualCompany.ShippersPhone ?? load.ShipperContacts.ToString() : "";
                    pickupFields["[shipper]"] = load.ShowPickupAddress ? load.ShipperCompany : $"Driver must Call {actualCompany.WorkPhone} for Dispatch";
                }
            }

            var loadDeliveries = children.Any() ? children : new List<LoadViewModel>() { load };
            foreach (var l in loadDeliveries) {
                var deliveryFields = deliveriesFields.FirstOrDefault(e => e["[destination_city]"] == l.DestinationCity &&
                                                                          e["[delivery_date]"] == l.DeliveryFullDateTime);
                if (deliveryFields == null) {
                    var list = new Dictionary<string, string>();
                    list.Add("[load_id]", l.IndexString);
                    list.Add("[delivery_id]", l.DeliveryNumber);
                    list.Add("[delivery_date]", l.DeliveryFullDateTime);
                    list.Add("[destination_address]", l.DestinationAddress);
                    list.Add("[destination_city]", l.DestinationCity);
                    list.Add("[weight]", NullableWeightToString(l.Weight, l.WeightType));
                    list.Add("[length]", l.Length.NullableLengthToString());
                    list.Add("[pallets]", l.Pallets.NullableToString());
                    list.Add("[receiver_phone]", l.ReceiverContacts.ToString());
                    list.Add("[receiver]", l.ReceiverCompany);
                    deliveriesFields.Add(list);
                } else if (deliveryFields["[destination_city]"] == load.DestinationCity &&
                           deliveryFields["[delivery_date]"] == load.DeliveryFullDateTime) {
                    deliveryFields["[load_id]"] = load.IndexString;
                    deliveryFields["[delivery_id]"] = load.DeliveryNumber;
                    deliveryFields["[delivery_date]"] = load.DeliveryFullDateTime;
                    deliveryFields["[destination_address]"] = load.DestinationAddress;
                    deliveryFields["[destination_city]"] = load.DestinationCity;
                    deliveryFields["[weight]"] = NullableWeightToString(load.Weight, load.WeightType);
                    deliveryFields["[length]"] = load.Length.NullableLengthToString();
                    deliveryFields["[pallets]"] = load.Pallets.NullableToString();
                    deliveryFields["[receiver_phone]"] = load.ReceiverContacts.ToString();
                    deliveryFields["[receiver]"] = load.ReceiverCompany;
                }
            }

            #endregion [ Init fields ]

            #region [ Replace fields ]

            if (actualCompany.ConfirmationData == null) {
                throw new Exception("actualCompany.ConfirmationData IS NULL");
            }

            StringBuilder sb = new StringBuilder(actualCompany.ConfirmationData);

            var beginIndex = sb.IndexOf("[begin_pickup_template]");
            sb.Replace("[begin_pickup_template]", "");
            var endIndex = sb.IndexOf("[end_pickup_template]");
            sb.Replace("[end_pickup_template]", "");
            char[] pickupTemplate = new char[endIndex - beginIndex];
            sb.CopyTo(beginIndex, pickupTemplate, 0, pickupTemplate.Length);
            sb.Remove(beginIndex, pickupTemplate.Length);

            beginIndex = sb.IndexOf("[begin_delivery_template]");
            sb.Replace("[begin_delivery_template]", "");
            endIndex = sb.IndexOf("[end_delivery_template]");
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
                sb.Insert(beginIndex, template);
            }
            foreach (Dictionary<string, string> loadFields in pickupsFields) {
                var template = new StringBuilder(new String(pickupTemplate));
                foreach (var field in loadFields) {
                    template.Replace(field.Key, field.Value);
                }
                sb.Insert(beginIndex, template);
            }

            foreach (var field in companyFields) {
                sb.Replace(field.Key, field.Value);
            }

            #endregion [ Replace fields ]

            #region [ Insert logo and comments ]

            var providerRtf = new RtfFormatProvider();
            RadDocument docRtf = providerRtf.Import(sb.ToString());

            // Inserts comments
            var editor = new RadDocumentEditor(docRtf);
            DocumentTextSearch search = new DocumentTextSearch(docRtf);

            var position = search.Find(Regex.Escape("[comments]"));
            position.SetSelection(docRtf);

            foreach (RadDocument radDocument in comments) {
                // skips first break line
                if (radDocument != comments[0]) {
                    editor.InsertLineBreak();
                }
                radDocument.Selection.SelectAll();
                DocumentFragment frag = new DocumentFragment(radDocument.Selection);
                editor.InsertFragment(frag);
            }
            editor.InsertLineBreak();
            editor.Delete(true);

            // Inserts logo
            position = search.Find(Regex.Escape("[logo]"));
            position.SetSelection(docRtf);
            var newHeight = 50;
            Size size = new Size(actualCompany.LogoBig.Width * newHeight / actualCompany.LogoBig.Height, newHeight);
            ImageInline inline = new ImageInline(actualCompany.LogoBig.StreamSource, size, "png");
            editor.InsertInline(inline);

            // Fills header

            // NOTE: here was driver: if (driver != null) {
            var email = company.Contacts.GetByType(ContactType.Email);
            var fax = company.Contacts.GetByType(ContactType.Fax);
            var phone = company.Contacts.GetByType(ContactType.Phone);
            // new/old version 
            if (sb.IndexOf("[title8]") == -1) {
                var row = 0;
                if (email != null) {
                    ReplaceText(editor, $"[title{row}]", "EMAIL");
                    ReplaceText(editor, $"[value{row}]", ((ContactEmailViewModel)email).Email);
                    row++;
                }
                if (fax != null) {
                    ReplaceText(editor, $"[title{row}]", "FAX");
                    ReplaceText(editor, $"[value{row}]", ((ContactBasePhoneViewModel)fax).ToString());
                    row++;
                }
                if (phone != null) {
                    ReplaceText(editor, $"[title{row}]", "PHONE");
                    if (String.IsNullOrEmpty(((ContactPhoneViewModel)phone).Contact) == false) {
                        ReplaceText(editor, $"[value{row}]", $"{(ContactBasePhoneViewModel)phone} {((ContactPhoneViewModel)phone).Contact}");
                    } else {
                        ReplaceText(editor, $"[value{row}]", ((ContactBasePhoneViewModel)phone).ToString());
                    }
                    row++;
                }
                if (phone == null && companyDetails.ContainsKey("Phone:")) {
                    ReplaceText(editor, $"[title{row}]", "PHONE");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Phone:"]);
                    row++;
                }
                if (companyDetails.ContainsKey("Physical Address:") && companyDetails.ContainsKey("Mailing Address:")) {
                    if (companyDetails["Physical Address:"] == companyDetails["Mailing Address:"]) {
                        ReplaceText(editor, $"[title{row}]", "ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                        row++;
                        ReplaceText(editor, $"[title{row}]", "CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                        row++;
                    } else {
                        ReplaceText(editor, $"[title{row}]", "MAILING ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Mailing Address:"].Split('\n').First().Trim());
                        row++;
                        ReplaceText(editor, $"[title{row}]", "MAILING CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Mailing Address:"].Split('\n').Last().Trim());
                        row++;
                        ReplaceText(editor, $"[title{row}]", "PHYSICAL ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                        row++;
                        ReplaceText(editor, $"[title{row}]", "PHYSICAL CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                        row++;
                    }
                } else if (companyDetails.ContainsKey("Physical Address:")) {
                    ReplaceText(editor, $"[title{row}]", "ADDRESS");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                    row++;
                    ReplaceText(editor, $"[title{row}]", "CITY");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                    row++;
                }
                // sets sizes
                ReplaceText(editor, "[titlea]", "LENGTH");
                ReplaceText(editor, "[titleb]", "WEIGHT");
                ReplaceText(editor, "[titlec]", "PALLETS");
                ReplaceText(editor, "[valuea]", load.Length.NullableLengthToString());
                ReplaceText(editor, "[valueb]", NullableWeightToString(load.Weight, load.WeightType));
                ReplaceText(editor, "[valuec]", load.Pallets.ToString());
                // deletes unusing rows
                for (int i = row; i <= 7; i++) {
                    DeleteTableRow(editor, $"[title{i}]");
                }
                // NEW/old version 
            } else {
                // new/OLD version 
                var row = 7;
                if (email != null) {
                    ReplaceText(editor, $"[title{row}]", "EMAIL");
                    ReplaceText(editor, $"[value{row}]", ((ContactEmailViewModel)email).Email);
                    row--;
                }
                if (fax != null) {
                    ReplaceText(editor, $"[title{row}]", "FAX");
                    ReplaceText(editor, $"[value{row}]", ((ContactBasePhoneViewModel)fax).ToString());
                    row--;
                }
                if (phone != null) {
                    ReplaceText(editor, $"[title{row}]", "PHONE");
                    if (String.IsNullOrEmpty(((ContactPhoneViewModel)phone).Contact) == false) {
                        ReplaceText(editor, $"[value{row}]", $"{(ContactBasePhoneViewModel)phone} {((ContactPhoneViewModel)phone).Contact}");
                    } else {
                        ReplaceText(editor, $"[value{row}]", ((ContactBasePhoneViewModel)phone).ToString());
                    }
                    row--;
                }
                if (phone == null && companyDetails.ContainsKey("Phone:")) {
                    ReplaceText(editor, $"[title{row}]", "PHONE");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Phone:"]);
                    row--;
                }
                if (companyDetails.ContainsKey("Physical Address:") && companyDetails.ContainsKey("Mailing Address:")) {
                    if (companyDetails["Physical Address:"] == companyDetails["Mailing Address:"]) {
                        ReplaceText(editor, $"[title{row}]", "CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                        row--;
                        ReplaceText(editor, $"[title{row}]", "ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                        row--;
                    } else {
                        ReplaceText(editor, $"[title{row}]", "MAILING CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Mailing Address:"].Split('\n').Last().Trim());
                        row--;
                        ReplaceText(editor, $"[title{row}]", "MAILING ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Mailing Address:"].Split('\n').First().Trim());
                        row--;
                        ReplaceText(editor, $"[title{row}]", "PHYSICAL CITY");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                        row--;
                        ReplaceText(editor, $"[title{row}]", "PHYSICAL ADDRESS");
                        ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                        row--;
                    }
                } else if (companyDetails.ContainsKey("Physical Address:")) {
                    ReplaceText(editor, $"[title{row}]", "CITY");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').Last().Trim());
                    row--;
                    ReplaceText(editor, $"[title{row}]", "ADDRESS");
                    ReplaceText(editor, $"[value{row}]", companyDetails["Physical Address:"].Split('\n').First().Trim());
                    row--;
                }
                // sets sizes
                ReplaceText(editor, "[titlea]", "LENGTH");
                ReplaceText(editor, "[titleb]", "WEIGHT");
                ReplaceText(editor, "[titlec]", "PALLETS");
                ReplaceText(editor, "[valuea]", load.Length.NullableLengthToString());
                ReplaceText(editor, "[valueb]", NullableWeightToString(load.Weight, load.WeightType));
                ReplaceText(editor, "[valuec]", load.Pallets.ToString());
                // deletes unusing rows
                for (int i = row; i >= 0; i--) {
                    DeleteTableRow(editor, $"[title{i}]");
                }
                // deletes unusing table
                DeleteTable(editor, "[title8]");
            }
            // new/old version 


            // delete accessorial row
            if (rates.Any() == false) {
                position = search.Find(Regex.Escape("Total Carrier Pay:"));
                position.SetSelection(editor.Document);
                Paragraph paragraph = editor.Document.Selection.GetSelectedParagraphs().First();
                var table = paragraph.GetParentOfType<Table>();
                editor.DeleteTableRow((TableRow)table.Rows.First());
            }

            #endregion [ Insert logo and comments ]

            return editor.Document;
        }

        private void ReplaceText(RadDocumentEditor editor, string oldValue, string newValue) {
            DocumentTextSearch search = new DocumentTextSearch(editor.Document);
            TextRange position = search.Find(Regex.Escape(oldValue));
            position.SetSelection(editor.Document);
            if (String.IsNullOrEmpty(newValue) == false) {
                Paragraph paragraph = editor.Document.Selection.GetSelectedParagraphs().First();
                var span = paragraph.EnumerateChildrenOfType<Span>().FirstOrDefault();
                if (span != null) {
                    span.Text = newValue;
                } else {
                    throw new Exception("undefined 'span' element");
                }
                //editor.ChangeSpanStyle(paragraph.Style);
                //editor.Insert(newValue);
            } else {
                editor.Delete(false);
            }
        }

        private void DeleteTableRow(RadDocumentEditor editor, string value) {
            DocumentTextSearch search = new DocumentTextSearch(editor.Document);
            var position = search.Find(Regex.Escape(value));
            position.SetSelection(editor.Document);
            var bb = editor.Document.Selection.GetSelectedParagraphs();
            editor.DeleteTableRow((TableRow)bb.First().Parent.Parent);
        }
        private void DeleteTable(RadDocumentEditor editor, string value) {
            DocumentTextSearch search = new DocumentTextSearch(editor.Document);
            var position = search.Find(Regex.Escape(value));
            position.SetSelection(editor.Document);
            var bb = editor.Document.Selection.GetSelectedParagraphs();
            editor.DeleteTable((Table)bb.First().Parent.Parent.Parent);
        }

        /// <summary>
        ///     Inserts confirmation# and returns binary of PDF document </summary>
        /// <param name="docRtf">RadDocument same as return value</param>
        /// <param name="load">Load ViewMdoel</param>
        /// <returns>RadDocument</returns>
        public byte[] GenerateConfirmationPdf(RadDocument docRtf, LoadViewModel load) {
            var editor = new RadDocumentEditor(docRtf);

            DocumentTextSearch search = new DocumentTextSearch(docRtf);

            #region [ replaces signs ]

            const string signSpace = "_____________________________";

            var carrier_position = search.Find(Regex.Escape("[carrier_signature]"));
            docRtf.Selection.AddSelectionStart(carrier_position.StartPosition);
            docRtf.Selection.AddSelectionEnd(carrier_position.EndPosition);
            docRtf.DocumentVariables.Add("carrier_signature", $"{carrier_position.StartPosition.Location.X / docRtf.SectionDefaultPageSize.Width},{carrier_position.StartPosition.Location.Y / docRtf.SectionDefaultPageSize.Height}");
            editor.Insert(signSpace);

            var agent_position = search.Find(Regex.Escape("[agent_signature]"));
            docRtf.Selection.AddSelectionStart(agent_position.StartPosition);
            docRtf.Selection.AddSelectionEnd(agent_position.EndPosition);
            docRtf.DocumentVariables.Add("agent_signature", $"{agent_position.StartPosition.Location.X / docRtf.SectionDefaultPageSize.Width},{agent_position.StartPosition.Location.Y / docRtf.SectionDefaultPageSize.Height}");
            editor.Insert(signSpace);

            #endregion [ replaces signs ]

            // Converts to PDF
            var providerPdf = new PdfFormatProvider();
            providerPdf.ExportSettings.ContentsDeflaterCompressionLevel = 9;
            providerPdf.ExportSettings.ImagesDeflaterCompressionLevel = 9;

            providerPdf.ExportSettings.DocumentInfo.Keywords = $"carrier_signature={docRtf.DocumentVariables["carrier_signature"]};agent_signature={docRtf.DocumentVariables["agent_signature"]};";
            try {
                return providerPdf.Export(docRtf);

            } catch (Exception e) {
                throw;
            }
        }

        /// <summary>
        ///     Generates bill of lading </summary>
        public RadDocument GenerateBOL(DatCompanyViewModel company, ActualCompanyViewModel actualCompany, LoadViewModel load, List<LoadViewModel> children, DatDriverViewModel driver) {

            #region [ Init fields ]

            var OrderFields = new Dictionary<string, string>();
            OrderFields.Add("[confirmation_id]", load.IndexString);
            var DriverFields = new Dictionary<string, string>();
            DriverFields.Add("[driver_name]", driver?.Name);
            DriverFields.Add("[truck_number]", driver?.TruckNumber);
            DriverFields.Add("[trailer_number]", driver?.TrailerNumber);
            var companyFields = new Dictionary<string, string>();
            companyFields.Add("[now]", DateTime.Now.ToShortDateString());
            var eqStr = load.Equipment.GetDescription() ?? load.Equipment.ToString();
            if (load.Equipment == AutoCall.Common.Enums.AutoCallEquipmentType.Reefer && load.Temperature.HasValue) {
                //eqStr += $" {load.Temperature.Value.ToString("+#;-#;0")}°F continue"; // ascii format
                eqStr += $" {load.Temperature.Value.ToString("+#;-#;0")}\\'b0F continue"; // rtf format
            }
            var customerFields = new Dictionary<string, string>();
            companyFields.Add("[equipment]", eqStr);

            companyFields.Add("[work_phone]", actualCompany.WorkPhone);
            companyFields.Add("[work_fax]", actualCompany.WorkFax);
            companyFields.Add("[work_email]", actualCompany.WorkEmail);

            companyFields.Add("[company_name]", actualCompany.LegalName);
            companyFields.Add("[company_mc]", actualCompany.McNumber.ToString());
            companyFields.Add("[company_address]", actualCompany.PhysicalAddress.Split('\n').First().Trim());
            companyFields.Add("[company_city]", actualCompany.PhysicalAddress.Split('\n').Last().Trim());

            var weightType = load.WeightType == WeightType.Lbs ? WeightType.Lbs.GetDescription() : String.Empty;
            companyFields.Add("[t_weight]", load.Weight != 0 ? load.Weight.ToString() + weightType : "");
            companyFields.Add("[t_length]", load.Length != 0 ? load.Length.ToString() : "");
            companyFields.Add("[t_pallets]", load.Pallets != 0 ? load.Pallets.ToString() : "");
            var LoadsFields = new List<Dictionary<string, string>>();

            var BillFields = new Dictionary<string, string>();

            BillFields.Add("[pickup_date]", load.PickupFullDateTime);
            BillFields.Add("[origin_address]", load.OriginAddress);
            BillFields.Add("[origin_city]", load.OriginCity);
            BillFields.Add("[pickup_id]", String.IsNullOrEmpty(load.PickupNumber) == false ? $"PU#: {load.PickupNumber}" : null);

            BillFields.Add("[delivery_date]", load.DeliveryFullDateTime);
            BillFields.Add("[destination_address]", load.DestinationAddress);
            BillFields.Add("[destination_city]", load.DestinationCity);
            BillFields.Add("[delivery_id]", String.IsNullOrEmpty(load.DeliveryNumber) == false ? $"Del#: {load.DeliveryNumber}" : null);

            //var commetns =
            var list = new Dictionary<string, string>();
            list.Add("[load_id]", String.IsNullOrEmpty(load.BOLNumber) == false ? load.BOLNumber : load.IndexString);
            list.Add("[bill_of_lading_id]", String.IsNullOrEmpty(load.BOLNumber) == false ? load.BOLNumber : load.IndexString);
            list.Add("[weight]", load.Weight.NullableToString() + weightType);
            list.Add("[length]", load.Length.NullableToString());
            list.Add("[pallets]", load.Pallets.NullableToString());
            list.Add("[receiver]", load.ReceiverCompany);
            list.Add("[shipper]", load.ShipperCompany);
            LoadsFields.Add(list);
            companyFields.Add("[load_rate]", $"{load.Currency.GetDescription()}{load.Rate:N2}");

            #endregion [ Init fields ]

            #region [ Replace fields ]

            StringBuilder sb = new StringBuilder(actualCompany.BOLData);

            LoadsFields.Reverse();
            foreach (Dictionary<string, string> loadFields in LoadsFields) {
                foreach (var field in loadFields) {
                    sb.ReplaceFirst(field.Key, field.Value);
                }
            }
            sb.Replace("[load_id]", "");
            sb.Replace("[length]", "");
            sb.Replace("[weight]", "");
            sb.Replace("[pallets]", "");

            foreach (var field in BillFields) {
                sb.Replace(field.Key, field.Value);
            }

            foreach (var field in companyFields) {
                sb.Replace(field.Key, field.Value);
            }

            foreach (var field in OrderFields) {
                sb.Replace(field.Key, field.Value);
            }

            foreach (var field in DriverFields) {
                sb.Replace(field.Key, field.Value);
            }

            #endregion [ Replace fields ]

            #region [ Insert logo ]

            var providerRtf = new RtfFormatProvider();
            RadDocument docRtf = providerRtf.Import(sb.ToString());

            var editor = new RadDocumentEditor(docRtf);
            DocumentTextSearch search = new DocumentTextSearch(docRtf);
            var position = search.Find(Regex.Escape("[logo]"));
            position.SetSelection(docRtf);
            var newHeight = 75;
            Size size = new Size(actualCompany.LogoBig.Width * newHeight / actualCompany.LogoBig.Height, newHeight);
            ImageInline inline = new ImageInline(actualCompany.LogoBig.StreamSource, size, "png");
            editor.InsertInline(inline);

            #endregion [ Insert logo ]

            return docRtf;
        }

        /// <summary>
        ///     Converts RadDocument to PDF binary </summary>
        public byte[] GenerateBOLPdf(RadDocument document) {
            var editor = new RadDocumentEditor(document);
            var providerPdf = new PdfFormatProvider();
            providerPdf.ExportSettings.ContentsDeflaterCompressionLevel = 9;
            providerPdf.ExportSettings.ImagesDeflaterCompressionLevel = 9;
            return providerPdf.Export(document);
        }

        /// <summary>
        /// Generates invoicedocument. </summary>
        /// <param name="actualCompany">Selected Actual Company View Model on MainVindow</param>
        /// <param name="load">Load view Models</param>
        /// <returns>RadDocument</returns>
        public RadDocument GenerateInvoice(
            ActualCompanyViewModel actualCompany,
            CensusCompanyViewModel customer,
            LoadViewModel load, List<LoadViewModel> children,
            FactoringCompanyViewModel factoringCompany) {
            #region [ Init fields ]

            var customerFields = new Dictionary<string, string>();
            if (customer != null) {
                customerFields.Add("[customer_name]", customer.NAME);
                customerFields.Add("[customer_address]", customer.PHY_STR);
                customerFields.Add("[customer_city]", $"{customer.PHY_CITY}, {customer.PHY_ST} {customer.PHY_ZIP}");
            } else {
                customerFields.Add("[customer_name]", load.Company);
                customerFields.Add("[customer_address]", "?");
                customerFields.Add("[customer_city]", "?");
            }

            var companyFields = new Dictionary<string, string>();
            companyFields.Add("[date]", DateTime.Now.ToShortDateString());
            companyFields.Add("[due_date]", DateTime.Now.AddMonths(1).ToShortDateString());
            var eqStr = load.Equipment.GetDescription() ?? load.Equipment.ToString();
            if (load.Equipment == AutoCallEquipmentType.Reefer && load.Temperature.HasValue) {
                eqStr += $" {load.Temperature.Value.ToString("+#;-#;0")}\\'b0F continue"; // rtf format
            }
            companyFields.Add("[company_phone]", actualCompany.WorkPhone);
            companyFields.Add("[company_mc]", $"MC#{actualCompany.McNumber}");
            companyFields.Add("[equipment]", eqStr);
            var loadFields = new Dictionary<string, string>();
            var comments = new List<RadDocument>();
            loadFields.Add("[load_id]", load.IndexString);
            loadFields.Add("[pro_id]", load.RecordId);
            loadFields.Add("[load_pickup_date]", load.PickupFullDateTime);
            loadFields.Add("[load_delivery_date]", load.DeliveryFullDateTime);
            loadFields.Add("[load_origin_address]", load.OriginAddress);
            loadFields.Add("[load_destination_address]", load.DestinationAddress);
            loadFields.Add("[load_origin_city]", load.OriginCity);
            loadFields.Add("[load_destination_city]", load.DestinationCity);
            var weightType = load.WeightType == WeightType.Lbs ? WeightType.Lbs.GetDescription() : String.Empty;
            loadFields.Add("[load_weight]", load.Weight.NullableToString() + weightType);
            loadFields.Add("[load_length]", load.Length.NullableToString());
            loadFields.Add("[load_pallets]", load.Pallets.NullableToString());
            loadFields.Add("[load_line_haul]", $"{load.Currency.GetDescription()}{load.LineHaul:N2}");
            loadFields.Add("[load_rate]", $"{load.Currency.GetDescription()}{load.Rate:N2}");
            loadFields.Add("[accessorial_label]",
                String.Join(@"\par ", load.Gains.Where(e => e.Cost != null).Select(e => $"{e.TypeDescription()}:")));
            loadFields.Add("[accessorial_value]",
                String.Join(@"\par ", load.Gains.Where(e => e.Cost != null).Select(e => $"{load.Currency.GetDescription()}{e.Cost:N2}")));

            if (load.InvoiceNumber != null) {
                // TR30236TR30209 - regen
                if (!Regex.IsMatch(load.InvoiceNumber, @"^(?<prefix1>\D*)(?<index1>\d{1,8})(?<prefix2>\D*)(?<index2>\d{1,8})$")) {
                    loadFields.Add("[invoice_id]", load.InvoiceNumber);
                }
            }

            companyFields.Add("[company_name]", actualCompany.LegalName);
            companyFields.Add("[company_address]", actualCompany.MailingAddress.Split('\n').First().Trim());
            companyFields.Add("[company_city]", actualCompany.MailingAddress.Split('\n').Last().Trim());
            // factoring company
            if (factoringCompany.HasFactoringCompany) {
                companyFields.Add("[company_header]", "");
                companyFields.Add("[company_name2]", "");
                companyFields.Add("[company_address2]", "");
                companyFields.Add("[company_city2]", "");
                loadFields["[fc_name]"] = factoringCompany.CompanyName;
                loadFields["[fc_address]"] = factoringCompany.Address;
                loadFields["[fc_city]"] = factoringCompany.City;
                loadFields["[fc_phone]"] = !String.IsNullOrEmpty(factoringCompany.Phone) ?
                    StringExtension.ToPhone(factoringCompany.Phone) : String.Empty;
                loadFields["[fc_email]"] = !String.IsNullOrEmpty(factoringCompany.Email) ?
                    factoringCompany.Email : String.Empty;
                if (!String.IsNullOrEmpty(factoringCompany.Phone) && !String.IsNullOrEmpty(factoringCompany.Email)) {
                    loadFields["[fc_email]"] = " or " + loadFields["[fc_email]"];
                }
            } else {
                companyFields.Add("[company_header]", "Make checks payable to:");
                companyFields.Add("[company_name2]", companyFields["[company_name]"]);
                companyFields.Add("[company_address2]", companyFields["[company_address]"]);
                companyFields.Add("[company_city2]", companyFields["[company_city]"]);
                loadFields["[fc_header]"] = String.Empty;
                loadFields["[fc_body]"] = String.Empty;
                loadFields["[fc_name]"] = String.Empty;
                loadFields["[fc_address]"] = String.Empty;
                loadFields["[fc_city]"] = String.Empty;
                loadFields["[fc_phone]"] = String.Empty;
                loadFields["[fc_email]"] = String.Empty;
            }

            // adds comments as documetns
            var providerXaml = new XamlFormatProvider();
            try {
                var docXaml = providerXaml.Import(load.Comments);
                comments.Add(docXaml);
            } catch {
            }

            #endregion [ Init fields ]

            #region [ Replace fields ]

            StringBuilder sb = new StringBuilder(actualCompany.InvoiceData);

            foreach (var loadField in loadFields) {
                sb.Replace(loadField.Key, loadField.Value);
            }
            foreach (var field in companyFields) {
                sb.Replace(field.Key, field.Value);
            }
            foreach (var field in customerFields) {
                sb.Replace(field.Key, field.Value);
            }

            #endregion [ Replace fields ]

            #region [ Factoring Company ]

            if (factoringCompany.HasFactoringCompany == false) {
                var fcStart = sb.IndexOf("[fc_start]");
                var fcEnd = sb.IndexOf("[fc_end]");
                sb.Remove(fcStart, fcEnd - fcStart);
            }
            sb.Replace("[fc_start]", "");
            sb.Replace("[fc_end]", "");

            #endregion [ Factoring Company ]

            #region [ Insert logo and comments ]

            var providerRtf = new RtfFormatProvider();
            RadDocument docRtf = providerRtf.Import(sb.ToString());

            //// Inserts comments
            var editor = new RadDocumentEditor(docRtf);
            DocumentTextSearch search = new DocumentTextSearch(docRtf);
            //var position = search.Find(Regex.Escape("[comments]"));
            //position.SetSelection(docRtf);

            //foreach (RadDocument radDocument in comments) {
            //    // skips first break line
            //    if (radDocument != comments[0]) {
            //        editor.InsertLineBreak();
            //    }
            //    radDocument.Selection.SelectAll();
            //    DocumentFragment frag = new DocumentFragment(radDocument.Selection);
            //    editor.InsertFragment(frag);
            //}
            //editor.Delete(true);

            // Inserts logo
            TextRange position = search.Find(Regex.Escape("[logo]"));
            if (position != null) {
                position.SetSelection(docRtf);
                var newHeight = 75;
                Size size = new Size(actualCompany.LogoBig.Width * newHeight / actualCompany.LogoBig.Height, newHeight);
                ImageInline inline = new ImageInline(actualCompany.LogoBig.StreamSource, size, "png");
                editor.InsertInline(inline);
            }

            #endregion [ Insert logo and comments ]

            return editor.Document;
        }

        /// <summary>
        ///     Inserts invoice# and returns binary of PDF document </summary>
        /// <param name="document">RadDocument same as return value</param>
        /// <param name="load">Load ViewMdoel</param>
        /// <returns>RadDocument</returns>
        public byte[] GenerateInvoicePdf(RadDocument document, LoadViewModel load) {
            var editor = new RadDocumentEditor(document);
            // Replaces conf#
            DocumentTextSearch search = new DocumentTextSearch(document);
            var position = search.Find(Regex.Escape("[invoice_id]"));
            if (position != null) {
                document.Selection.AddSelectionStart(position.StartPosition);
                document.Selection.AddSelectionEnd(position.EndPosition);
                editor.Insert(load.InvoiceNumber);
            }
            // Converts to PDF
            var providerPdf = new PdfFormatProvider();
            providerPdf.ExportSettings.ContentsDeflaterCompressionLevel = 9;
            providerPdf.ExportSettings.ImagesDeflaterCompressionLevel = 9;
            return providerPdf.Export(document);
        }

        public static string NullableWeightToString(int? value, WeightType type) {
            if (value.HasValue && value != 0) return $"{value} {type.GetDescription()}";
            return "";
        }
    }
}
