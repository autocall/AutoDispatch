using System;
using System.Collections.Generic;
using System.Linq;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class ConfirmationViewModelItem : ViewModel {
        public string Name { get; set; }
        public string Replace { get; set; }
        public string Value { get; set; }
        public bool Show { get; set; } = true;
        public bool Editable { get; set; }

        public ConfirmationViewModelItem(string name, string replace, string value, bool editable = false) {
            Name = name;
            Replace = replace;
            Value = value;
            Editable = editable;
        }
    }

    public class ConfirmationViewModel : ViewModel {

        public List<ConfirmationViewModelItem> CompanyFields { get; set; }

        public List<List<ConfirmationViewModelItem>> LoadsFields { get; set; }

        public ConfirmationViewModel(DatCompanyViewModel company, IList<LoadViewModel> loads) {
            CompanyFields = new List<ConfirmationViewModelItem>();
            CompanyFields.Add(new ConfirmationViewModelItem("Date Now", "#now#", DateTime.Now.ToShortDateString()));
            CompanyFields.Add(new ConfirmationViewModelItem("Equipment", "#equipment#", loads[0].Equipment.GetDescription() ?? loads[0].Equipment.ToString()));
            CompanyFields.Add(new ConfirmationViewModelItem("Company Name", "#company_name#", company.Name));
            CompanyFields.Add(new ConfirmationViewModelItem("Company MC#", "#company_mc#", company.McNumber.ToString()));
            var weight = loads.Sum(l => l.Weight);
            var length = loads.Sum(l => l.Length);
            var pallets = loads.Sum(l => l.Pallets);
            CompanyFields.Add(new ConfirmationViewModelItem("Weight", "#t_weight#", weight != 0 ? weight.ToString() : "", true));
            CompanyFields.Add(new ConfirmationViewModelItem("Length", "#t_length#", length != 0 ? length.ToString() : "", true));
            CompanyFields.Add(new ConfirmationViewModelItem("Pallets", "#t_pallets#", pallets != 0 ? pallets.ToString() : "", true));
            LoadsFields = new List<List<ConfirmationViewModelItem>>();
            foreach (var load2 in loads) {
                var list = new List<ConfirmationViewModelItem>();
                list.Add(new ConfirmationViewModelItem("Pickup", "#pickup_date#", load2.PickupFullDateTime, true));
                list.Add(new ConfirmationViewModelItem("Delivery", "#delivery_date#", load2.DeliveryFullDateTime, true));
                list.Add(new ConfirmationViewModelItem("Origin Address", "#origin_address#", load2.OriginAddress, true));
                list.Add(new ConfirmationViewModelItem("Destination Address", "#destination_address#", load2.DestinationAddress, true));
                list.Add(new ConfirmationViewModelItem("Origin City", "#origin_city#", load2.OriginCity, true));
                list.Add(new ConfirmationViewModelItem("Destination City", "#destination _city#", load2.DestinationCity, true));
                list.Add(new ConfirmationViewModelItem("Weight", "#weight#", load2.Weight.NullableToString(), true));
                list.Add(new ConfirmationViewModelItem("Length", "#length#", load2.Length.NullableToString(), true));
                list.Add(new ConfirmationViewModelItem("Pallets", "#pallets#", load2.Pallets.NullableToString(), true));
                list.Add(new ConfirmationViewModelItem("Commants1", "#comments1#", load2.Comments, true));
                list.Add(new ConfirmationViewModelItem("Commants2", "#comments2#", load2.Comments2, true));
                list.Add(new ConfirmationViewModelItem("Shipper Phone", "#shipper_phone#", load2.ShipperContacts.ToString()));
                list.Add(new ConfirmationViewModelItem("Receiver Phone", "#receiver_phone#", load2.ReceiverContacts.ToString()));
                LoadsFields.Add(list);
            }
            var rate = loads.Sum(l => l.Rate);
            CompanyFields.Add(new ConfirmationViewModelItem("Rate", "#rate#", rate.NullableToString(), true));
        }
    }
}

