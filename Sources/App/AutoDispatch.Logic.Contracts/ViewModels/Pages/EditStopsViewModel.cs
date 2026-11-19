using System;
using System.Collections.ObjectModel;
using AutoCall.Common.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.ViewModels.Pages {

    public class StopViewModel : ViewModel {
        public DateTime ArriveDate { get; set; }
        public TimeSpan ArriveFrom { get; set; }
        public TimeSpan ArriveTo { get; set; }

        public string Arrive {
            get { return DispatchHelper.DateTimeToAppointment(ArriveDate, ArriveFrom, ArriveTo); }
        }

        public LoadLocationViewModel Location { get; set; }

        [JsonIgnore]
        public LoadViewModel Load { get; set; }
        public Guid LoadId { get; set; }
        public StopType Type { get; set; }
        public byte Order { get; set; }
    }

    public class EditStopsViewModel : ViewModel {
        public TrailerViewModel Trailer { get; set; }
        public ObservableCollection<StopViewModel> Stops { get; set; }
    }
}
