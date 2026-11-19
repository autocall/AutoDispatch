using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.Settings;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Represents converter "default load values" -> "view model" </summary>
    /// <summary> Значения нового груза из настроек (V1 Settings/DefaultPattern/Load_SettingsToVm, Load_VmToSettings) </summary>
    public static class DefaultLoadSettingsConverter {
        public static LoadViewModel ToVm(DefaultLoadSettings source) {
            if (source == null) return null;
            var result = new LoadViewModel() { CurrentLocation = null };
            //result.OriginCity = source.OriginCity;
            //result.DestinationCity = source.DestinationCity;
            //result.OriginAddress = source.OriginAddress;
            //result.DestinationAddress = source.DestinationAddress;
            //result.OriginLocation = ServiceLocator.Current.GetInstance<IMapper>().Map<LocationViewModel>(source.OriginLocation); 
            //result.DestinationLocation = ServiceLocator.Current.GetInstance<IMapper>().Map<LocationViewModel>(source.DestinationLocation);
            //result.Contacts = ServiceLocator.Current.GetInstance<IMapper>().Map<string, ContactsViewModel>(source.Contacts);
            result.Equipment = (AutoCallEquipmentType)source.Equipment;
            //result.Weight = source.Weight;
            result.Length = source.Length;
            //result.Pallets = source.Pallets;
            //result.PickupFrom = source.PickupFrom.TimeOfDay;
            //result.PickupTo = source.PickupTo.TimeOfDay;
            //result.DeliveryFrom = source.DeliveryFrom.TimeOfDay;
            //result.DeliveryTo = source.DeliveryTo.TimeOfDay;
            //result.Comments = source.Comments;
            //result.Comments2 = source.Comments2;
            //result.Consolidation = source.Consolidation;
            //result.BrokerId = source.BrokerId;
            //result.BrokerCompany = source.BrokerCompany;
            //result.ReceiverId = source.ReceiverId;
            //result.ShipperId = source.ShipperId;
            //result.ReceiverCompany = source.ReceiverCompany;
            //result.ShipperCompany = source.ShipperCompany;
            result.LoadType = (LoadType)source.LoadType;
            //result.Rate = source.Rate;
            //result.McNumber = source.McNumber;
            //result.Currency = (CurrencyType)source.Currency;
            result.TrailerType = (TrailerType)source.TrailerType;
            result.Temperature = source.Temperature;
            //result.ShipperContacts = ServiceLocator.Current.GetInstance<IMapper>().Map<string, ContactsViewModel>(source.ShipperContacts);
            //result.ReceiverContacts = ServiceLocator.Current.GetInstance<IMapper>().Map<string, ContactsViewModel>(source.ReceiverContacts);

            return result;
        }
        public static DefaultLoadSettings ToSettings(LoadViewModel source) {
            if (source == null) return null;
            var result = new DefaultLoadSettings();
            //result.OriginCity = source.OriginCity;
            //result.DestinationCity = source.DestinationCity;
            //result.OriginAddress = source.OriginAddress;
            //result.DestinationAddress = source.DestinationAddress;
            //result.OriginLocation = ServiceLocator.Current.GetInstance<IMapper>().Map<LocationDto>(source.OriginLocation);
            //result.DestinationLocation = ServiceLocator.Current.GetInstance<IMapper>().Map<LocationDto>(source.DestinationLocation);
            //result.Contacts = ServiceLocator.Current.GetInstance<IMapper>().Map<ContactsViewModel, string>(source.Contacts);
            result.Equipment = (int) source.Equipment;
            //result.Weight = source.Weight;
            result.Length = source.Length;
            //result.Pallets = source.Pallets;
            //result.PickupFrom = source.PickupDate.Date + source.PickupFrom;
            //result.PickupTo = source.PickupDate.Date + source.PickupTo;
            //result.DeliveryFrom = source.DeliveryDate.Date + source.DeliveryFrom;
            //result.DeliveryTo = source.DeliveryDate.Date + source.DeliveryTo;
            //result.Comments = source.Comments;
            //result.Comments2 = source.Comments2;
            //result.Consolidation = source.Consolidation;
            //result.BrokerId = source.BrokerId;
            //result.BrokerCompany = source.BrokerCompany;
            //result.ReceiverId = source.ReceiverId;
            //result.ShipperId = source.ShipperId;
            //result.ReceiverCompany = source.ReceiverCompany;
            //result.ShipperCompany = source.ShipperCompany;
            result.LoadType = (byte) source.LoadType;
            //result.Rate = source.Rate;
            //result.McNumber = source.McNumber;
            //result.Currency = (byte) source.Currency;
            result.TrailerType = (byte) source.TrailerType;
            result.Temperature = source.Temperature;
            //result.ShipperContacts = ServiceLocator.Current.GetInstance<IMapper>().Map<ContactsViewModel, string>(source.ShipperContacts);
            //result.ReceiverContacts = ServiceLocator.Current.GetInstance<IMapper>().Map<ContactsViewModel, string>(source.ReceiverContacts);
            return result;
        }
    }
}