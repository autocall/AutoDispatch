using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Enums;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class LoadDivideDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private readonly DispatchService _service;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public LoadDivideViewModel Model {
            get { return DataContext as LoadDivideViewModel; }
            private set { DataContext = value; }
        }
        public LoadDivideViewModel Result;

        public LoadDivideDialog(DispatchService service, LoadViewModel load, LoadViewModel parent) {

            Model = new LoadDivideViewModel(load, parent);
            _service = service;

            InitializeComponent();

            Title += $" #{load.IndexString}";
        }

        private void LoadDivideDialog_OnLoaded(object sender, RoutedEventArgs e) {
            if (Model.InitialLoad.ContainerType != ContainerType.None && Model.InitialLoad.ContainerType != ContainerType.Quantity) {
                MessageBoxEx.Alert(this, "You cannot divide this cargo");
                Close();
            }
            Model.LockLength1 = false;
            Model.LockLength2 = false;
            Model.LockWeight1 = false;
            Model.LockWeight2 = false;
            Model.LockLineHaul1 = false;
            Model.LockLineHaul2 = false;
            Model.LockPallets1 = false;
            Model.LockPallets2 = false;
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.InitialLoad.Weight != 0 && (Model.Load1.Weight == 0 || Model.Load2.Weight == 0)) {
                MessageBoxEx.Alert(this, "Please enter a Weight");
                return;
            }
            if (Model.InitialLoad.Length != 0 && (Model.Load1.Length == 0 || Model.Load2.Length == 0)) {
                MessageBoxEx.Alert(this, "Please enter a Length");
                return;
            }
            if (Model.InitialLoad.Pallets != 0 && (Model.Load1.Pallets == 0 || Model.Load2.Pallets == 0)) {
                MessageBoxEx.Alert(this, "Please enter Pallets");
                return;
            }
            if (Model.InitialLoad.LineHaul != 0 && (Model.Load1.LineHaul == 0 || Model.Load2.LineHaul == 0)) {
                MessageBoxEx.Alert(this, "Please enter a Line Haul");
                return;
            }
            //if (Model.InitialLoad.DatCompanyId != null || Model.InitialLoad.DatDriverId != null || Model.InitialLoad.TrailerId != null) {
            //    if (Model.InitialLoad.Status != LoadStatus.AtPickUp) {
            //        MessageBoxEx.Alert(this, "Operation is not available The load already assigned");
            //        return;
            //    }
            //}
            //if (Model.InitialLoad.Status != LoadStatus.None && Model.InitialLoad.Status != LoadStatus.AtPickUp) {
            //    MessageBoxEx.Alert(this, "Change status to 'none'");
            //    return;
            //}
            if (Model.InitialLoad.ParentId == null) {
                Model.InitialLoad.ContainerType = ContainerType.Quantity;
                Model.Load1.EntityId = Guid.NewGuid();
                Model.Load2.EntityId = Guid.NewGuid();
                Model.Load1.ParentId = Model.InitialLoad.EntityId;
                Model.Load2.ParentId = Model.InitialLoad.EntityId;

                var loads = new List<LoadViewModel>() {
                    Model.InitialLoad,
                    Model.Load1,
                    Model.Load2,
                };
                //base.BusyStart();
                //_service.SaveLoads(loads, SaveLoads_Completed);
                DialogResult = true;
            } else {
                if (Model.Parent.ContainerType != ContainerType.Quantity) {
                    MessageBoxEx.Alert(this, "Operation is not available");
                    return;
                }

                Model.Load1.EntityId = Model.InitialLoad.EntityId;
                Model.Load2.EntityId = Guid.NewGuid();

                var loads = new List<LoadViewModel>() {
                    Model.Load1,
                    Model.Load2,
                };
                //base.BusyStart();
                //_service.SaveLoads(loads, SaveLoads_Completed);
                DialogResult = true;
            }
            // связь осталась, после закрытия окна и работает, когда редактирую подгруз
            this.Result = Model;
            Model = null;
            Close();
        }

        // NOTES: проблема с округлением. сделал через float -> int и max 1000, если вдруг какая то цифра будет больше 100.
        private void Load1Length_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockLength1) return; else Model.LockLength1 = true;
            if (Model.Load1.Length != null && Model.InitialLoad.Length != null && Model.Load1.Length != 0 && Model.InitialLoad.Length != 0)
                Model.Value = (int)Math.Round(1000f * (int)Model.Load1.Length / (int)Model.InitialLoad.Length);
            Model.LockLength1 = false;
        }
        private void Load2Length_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockLength2) return; else Model.LockLength2 = true;
            if (Model.Load2.Length != null && Model.InitialLoad.Length != null && Model.Load2.Length != 0 && Model.InitialLoad.Length != 0)
                Model.Value = (int)Math.Round(1000f * ((int)Model.InitialLoad.Length - (int)Model.Load2.Length) / (int)Model.InitialLoad.Length);
            Model.LockLength2 = false;
        }
        private void Load1Weight_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockWeight1) return; else Model.LockWeight1 = true;
            if (Model.Load1.Weight != null && Model.InitialLoad.Weight != null && Model.Load1.Weight != 0 && Model.InitialLoad.Weight != 0)
                Model.Value = (int)Math.Round(1000f * (int)Model.Load1.Weight / (int)Model.InitialLoad.Weight);
            Model.LockWeight1 = false;
        }
        private void Load2Weight_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockWeight2) return; else Model.LockWeight2 = true;
            if (Model.Load2.Weight != null && Model.InitialLoad.Weight != null && Model.Load2.Weight != 0 && Model.InitialLoad.Weight != 0)
                Model.Value = (int)Math.Round(1000f * ((int)Model.InitialLoad.Weight - (int)Model.Load2.Weight) / (int)Model.InitialLoad.Weight);
            Model.LockWeight2 = false;
        }
        private void Load1Pallets_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockPallets1) return; else Model.LockPallets1 = true;
            if (Model.Load1.Pallets != null && Model.InitialLoad.Pallets != null && Model.Load1.Pallets != 0 && Model.InitialLoad.Pallets != 0)
                Model.Value = (int)Math.Round(1000f * (int)Model.Load1.Pallets / (int)Model.InitialLoad.Pallets);
            Model.LockPallets1 = false;
        }
        private void Load2Pallets_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockPallets2) return; else Model.LockPallets2 = true;
            if (Model.Load2.Pallets != null && Model.InitialLoad.Pallets != null && Model.Load2.Pallets != 0 && Model.InitialLoad.Pallets != 0)
                Model.Value = (int)Math.Round(1000f * ((int)Model.InitialLoad.Pallets - (int)Model.Load2.Pallets) / (int)Model.InitialLoad.Pallets);
            Model.LockPallets2 = false;
        }
        private void Load1LineHaul_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockLineHaul1) return; else Model.LockLineHaul1 = true;
            if (Model.Load1.LineHaul != null && Model.InitialLoad.LineHaul != null && Model.Load1.LineHaul != 0 && Model.InitialLoad.LineHaul != 0)
                Model.Value = (int)Math.Round(1000f * (float)Model.Load1.LineHaul / (float)Model.InitialLoad.LineHaul);
            Model.LockLineHaul1 = false;
        }
        private void Load2LineHaul_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model == null) return;
            if (Model.LockLineHaul2) return; else Model.LockLineHaul2 = true;
            if (Model.Load2.LineHaul != null && Model.InitialLoad.LineHaul != null && Model.Load2.LineHaul != 0 && Model.InitialLoad.LineHaul != 0)
                Model.Value = (int)Math.Round(1000f * ((float)Model.InitialLoad.LineHaul - (float)Model.Load2.LineHaul) / (float)Model.InitialLoad.LineHaul);
            Model.LockLineHaul2 = false;
        }
    }
}
