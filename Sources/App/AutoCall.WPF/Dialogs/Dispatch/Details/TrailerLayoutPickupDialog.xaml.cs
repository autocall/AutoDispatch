using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Logic.Services;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoCall.WPF.Enums;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class TrailerLayoutPickupDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar {
            get { return progressBar; }
        }

        protected override TextBlock ErrorTextBlock {
            get { return tbError; }
        }

        protected override Button OkButton {
            get { return btnOk; }
        }

        protected override Button CloseButton {
            get { return btnClose; }
        }

        /// <summary>
        ///     Call service instance </summary>
        protected ICallService _callService;

        private DispatchService _dispatchService;
        private IMapper _mapper;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public TrailerLayoutPickupViewModel Model {
            get { return DataContext as TrailerLayoutPickupViewModel; }
            set { DataContext = value; }
        }

        public LoadViewModel Result { get; set; }

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public TrailerLayoutPickupDialog(
            DispatchService dispatchService, ICallService callService, IMapper mapper,
            TrailerViewModel cloneTrailer, LoadViewModel originLoad, LoadViewModel cloneLoad, DivideType divideType) {
            _dispatchService = dispatchService;
            _mapper = mapper;
            _callService = callService;

            InitializeComponent();

            // Replace pallets to pallets from trailer
            cloneLoad.Layout.Clear();
            foreach (var cell in cloneTrailer.Layout) {
                foreach (var pallet in cell.Pallets) {
                    if (pallet.Load.EntityId == cloneLoad.EntityId) {
                        cloneLoad.Layout.Add(pallet);
                        pallet.Load = cloneLoad;
                    }
                }
            }

            Model = new TrailerLayoutPickupViewModel() { Load = cloneLoad, Trailer = cloneTrailer, DivideType = divideType };
            Model.AvailablePallets = new ObservableCollection<LoadPalletViewModel>();
            if (divideType == DivideType.Pallets) {
                if (originLoad.Pallets != null) {
                    for (int i = 0; i < originLoad.Pallets; i++) {
                        Model.AvailablePallets.Add(new LoadPalletViewModel(originLoad));
                    }
                } else {
                    MessageBoxEx.Error(this, "Pallets can not be null");
                }
            }
            if (divideType == DivideType.Length) {
                if (originLoad.Length != null) {
                    for (int i = 0; i < AutoDispatch.Logic.Helpers.DispatchHelper.LengthToPallets(originLoad.Length.Value); i++) {
                        Model.AvailablePallets.Add(new LoadPalletViewModel(originLoad));
                    }
                } else {
                    MessageBoxEx.Error(this, "Length can not be null");
                }
            }
        }

        #endregion [ .ctor ]

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.Load.Pallets != null && Model.AvailablePallets.Any()) {
                var loaded = Model.AvailablePallets.Count(p => p.Changed);
                var total = Model.AvailablePallets.Count;
                if (loaded != total) {
                    if (MessageBox.Show($"Attention you loaded {loaded} out of {total} pallets!", "", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                        return;
                    }
                }
            }

            Model.Load.Status = LoadStatus.InTransit;
            _dispatchService.SaveLoad(Model.Load, SaveCompleted);

            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<LoadViewModel> wrapper) {
            if (wrapper.Status == ResponseStatus.Success) {
                // NOTE: will be used in TrailerLayoutDetailsDialog
                Result = wrapper.Data;
            }
            await base.SaveCompleted(wrapper);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void EditBookLoadDialog_OnLoaded(object sender, RoutedEventArgs e) {

        }

        private void btnMoveAll_Clicked(object sender, RoutedEventArgs args) {
            foreach (LoadPalletViewModel pallet in Model.AvailablePallets.Where(e => !e.Changed).ToList()) {
                var cell = Model.Trailer.Layout.OrderBy(c => c.Pallets.Select(p => (int)p.Level).DefaultIfEmpty(-1).Max(l => l)).First();
                if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                    pallet.Index = (byte)cell.Index;
                    pallet.Level = (byte)cell.Pallets.Count;
                    pallet.Changed = true;

                    cell.Pallets.Add(pallet);
                    Model.Load.Layout.Add(pallet);
                } else {
                    MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                    return;
                }
            }
        }

        private void AddItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var dragData = new DataObject(DragPalletSourceType.New.ToString(), new LoadPalletViewModel(this.Model.Load));
            DragDropEffects finalDropEffect = DragDrop.DoDragDrop((DependencyObject)e.OriginalSource, dragData, DragDropEffects.Move);
        }

        private void MoveItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pallet = (LoadPalletViewModel)((FrameworkElement)sender).DataContext;
            var dragData = new DataObject(DragPalletSourceType.Free.ToString(), pallet);
            DragDropEffects finalDropEffect = DragDrop.DoDragDrop((DependencyObject)e.OriginalSource, dragData, DragDropEffects.Move);
        }

        private void Trailer_OnDrop(object sender, DragEventArgs e) {
            var tl = (TrailerLayout)sender;
            var ui = UIElementHelper.FindVisualParent<ContentControl>(e.OriginalSource as UIElement);
            var cell = ui?.DataContext as TrailerCellViewModel;

            if (cell != null) {
                // from trailer
                var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Contains(pallet) == false) {
                        if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                            pallet.Index = (byte)cell.Index;
                            pallet.Level = (byte)cell.Pallets.Count;
                            pallet.Changed = true;

                            AutoDispatch.Logic.Helpers.DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                            cell.Pallets.Add(pallet);
                        } else {
                            MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                        }
                    }
                }
                // from free
                pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                        pallet.Index = (byte)cell.Index;
                        pallet.Level = (byte)cell.Pallets.Count;
                        pallet.Changed = true;

                        cell.Pallets.Add(pallet);
                        Model.Load.Layout.Add(pallet);
                    } else {
                        MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                    }
                }
                // from new
                pallet = e.Data.GetData(DragPalletSourceType.New.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                        pallet.Index = (byte)cell.Index;
                        pallet.Level = (byte)cell.Pallets.Count;
                        pallet.Changed = true;

                        cell.Pallets.Add(pallet);
                        Model.Load.Layout.Add(pallet);
                    } else {
                        MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                    }
                }
                this.UpdateTrigger();
            }
        }

        private void RemoveLoad_OnDrop(object sender, DragEventArgs e) {
            var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                if (pallet.Load.EntityId != Model.Load.EntityId) {
                    MessageBoxEx.Alert(this, "this item can not be removed");
                    return;
                }
                AutoDispatch.Logic.Helpers.DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                Model.Load.Layout.Remove(pallet);
                pallet.Changed = true;
            }

            pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                pallet.Changed = true;
            }
            this.UpdateTrigger();
        }

        private void freeList_Drop(object sender, DragEventArgs e) {
            var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                var targetPallet = Model.AvailablePallets.FirstOrDefault(p => p.Changed);
                if (targetPallet != null) {
                    targetPallet.Changed = false;
                } else {
                    Model.AvailablePallets.Add(pallet);
                }
                AutoDispatch.Logic.Helpers.DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                Model.Load.Layout.Remove(pallet);
            }
            pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
            if (pallet != null) {

            }
            pallet = e.Data.GetData(DragPalletSourceType.New.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                var targetPallet = Model.AvailablePallets.FirstOrDefault(p => p.Changed);
                if (targetPallet != null) {
                    targetPallet.Changed = false;
                } else {
                    Model.AvailablePallets.Add(pallet);
                }
            }
            this.UpdateTrigger();
        }

        private void UpdateTrigger() {
            // Update all cells
            foreach (var cell1 in Model.Trailer.Layout) {
                foreach (var pallet1 in cell1.Pallets) {
                    pallet1.UpdateTrigger();
                }
            }
        }
    }
}