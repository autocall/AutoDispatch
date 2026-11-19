using System;
using System.Collections.Generic;
using System.Linq;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class BOLViewModelItem : ViewModel {
        public string Name { get; set; }
        public string Replace { get; set; }
        public string Value { get; set; }
        public bool Show { get; set; } = true;
        public bool Editable { get; set; }

        public BOLViewModelItem(string name, string replace, string value, bool editable = false) {
            Name = name;
            Replace = replace;
            Value = value;
            Editable = editable;
        }
    }

    public class BOLViewModel : ViewModel {

        public List<ConfirmationViewModelItem> BillFields { get; set; }

        public List<ConfirmationViewModelItem> CompanyFields { get; set; }

        public List<ConfirmationViewModelItem> DriverFields { get; set; }

        public List<List<ConfirmationViewModelItem>> LoadsFields { get; set; }

        public BOLViewModel(DatCompanyViewModel company, DatDriverViewModel driver, IList<LoadViewModel> loads) {
            DriverFields = new List<ConfirmationViewModelItem>();
            DriverFields.Add(new ConfirmationViewModelItem("Driver", "#driver_name#", driver.Name));
            DriverFields.Add(new ConfirmationViewModelItem("Truck Number", "#truck_number#", driver.TruckNumber));
            DriverFields.Add(new ConfirmationViewModelItem("Trailer Number", "#trailer_number#", driver.TrailerNumber));
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

            var origin = loads.OrderBy(l => l.PickupFrom).ThenBy(l => l.PickupDate.Date).First();
            var destin = loads.OrderBy(l => l.DeliveryTo).ThenBy(l => l.DeliveryDate.Date).Last();
            BillFields = new List<ConfirmationViewModelItem>();

            BillFields.Add(new ConfirmationViewModelItem("Pickup", "#pickup_date#", origin.PickupFullDateTime, true));
            BillFields.Add(new ConfirmationViewModelItem("Origin Address", "#origin_address#", origin.OriginAddress, true));
            BillFields.Add(new ConfirmationViewModelItem("Origin City", "#origin_city#", origin.OriginCity, true));

            BillFields.Add(new ConfirmationViewModelItem("Delivery", "#delivery_date#", destin.DeliveryFullDateTime, true));
            BillFields.Add(new ConfirmationViewModelItem("Destination Address", "#destination_address#", destin.DestinationAddress, true));
            BillFields.Add(new ConfirmationViewModelItem("Destination City", "#destination_city#", destin.DestinationCity, true));

            foreach (var load2 in loads) {
                var list = new List<ConfirmationViewModelItem>();
                list.Add(new ConfirmationViewModelItem("Load ID", "#load_id#", load2.EntityId.ToString().Substring(0, 6)));
                list.Add(new ConfirmationViewModelItem("Weight", "#weight#", load2.Weight.NullableToString(), true));
                list.Add(new ConfirmationViewModelItem("Length", "#length#", load2.Length.NullableToString(), true));
                list.Add(new ConfirmationViewModelItem("Pallets", "#pallets#", load2.Pallets.NullableToString(), true));
                LoadsFields.Add(list);
            }
        }
    }
}

