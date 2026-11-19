using AutoDispatch.Logic.ViewModels.Items;
using System;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class LoadDivideViewModel : BaseViewModel {
        private int _value;
        public LoadViewModel Parent { get; set; }
        public LoadViewModel InitialLoad { get; set; }
        public LoadViewModel Load1 { get; set; }
        public LoadViewModel Load2 { get; set; }

        public bool LockValue = false;
        public bool LockLength1 = true;
        public bool LockLength2 = true;
        public bool LockWeight1 = true;
        public bool LockWeight2 = true;
        public bool LockLineHaul1 = true;
        public bool LockLineHaul2 = true;
        public bool LockPallets1 = true;
        public bool LockPallets2 = true;

        public string Title => InitialLoad.Pallets != null ? "Split Palletised Load" : "Split Floor Load ";

        /// <summary>
        ///     NOTES: временная доработка, сделал maximum 1000 вместо 100. 
        ///     алгоритм пропорциональной разбивки основан на Math.Round, любые значения меньше тысячи, будут разбиваться правильно
        /// </summary>
        public int Value {
            get { return _value; }
            set {
                if (_value != value) {
                    if (LockValue == true) {
                        return;
                    } else {
                        LockValue = true;
                    }

                    _value = value;

                    if (InitialLoad.Length != null) {
                        Load1.Length = (int)Math.Round((float)InitialLoad.Length * value / 1000f);
                        Load2.Length = InitialLoad.Length - Load1.Length;
                    }
                    if (InitialLoad.Weight != null) {
                        Load1.Weight = (int)Math.Round((float)InitialLoad.Weight * value / 1000f);
                        Load2.Weight = InitialLoad.Weight - Load1.Weight;
                    }
                    if (InitialLoad.Pallets != null) {
                        Load1.Pallets = (short)Math.Round((float)(InitialLoad.Pallets * value / 1000f));
                        Load2.Pallets = (short?)(InitialLoad.Pallets - Load1.Pallets);
                    }
                    if (InitialLoad.LineHaul != null) {
                        Load1.LineHaul = (int)Math.Round((float)InitialLoad.LineHaul * value / 1000f);
                        Load2.LineHaul = InitialLoad.LineHaul - Load1.LineHaul;
                    }
                    if (InitialLoad.DatRate != null) {
                        Load1.DatRate = (int)Math.Round((float)InitialLoad.DatRate * value / 1000f);
                        Load2.DatRate = InitialLoad.DatRate - Load1.DatRate;
                    }
                    OnPropertyChanged();
                    LockValue = false;
                }
            }
        }

        public LoadDivideViewModel(LoadViewModel load, LoadViewModel parent) {
            InitialLoad = load;
            Parent = parent;
            Load1 = (LoadViewModel)load.Clone();
            Load2 = (LoadViewModel)load.Clone();
            Load1.CarrierViewTracking = null;
            Load2.CarrierViewTracking = null;
            //Load1.DatRate = null;
            //Load2.DatRate = null;
            Load1.Expenses = null;
            Load2.Expenses = null;
            Load1.Gains = null;
            Load2.Gains = null;
            Load1.Index = null;
            Load2.Index = null;
            Load1.IndexExtra = null;
            Load2.IndexExtra = null;
            // I desided to copy properies to subloads. EditLoadDialog -> DispatchHelper.CopyContainerProperties
            //Load1.RecordId = null;
            //Load2.RecordId = null;
            //Load1.BrokerCompany = null;
            //Load2.BrokerCompany = null;
            //Load1.McNumber = null;
            //Load2.McNumber = null;
            //Load1.Contacts = null;
            //Load2.Contacts = null;

            Value = 500;
        }
    }
}
