using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoCall.Common;
using AutoCall.Logic.Services;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoCall.WPF.Enums;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Controls;
using DispatchHelper = AutoDispatch.Logic.Helpers.DispatchHelper;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class TrailerLayoutEditDialog : BaseDispatchDialog {
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

        public List<LoadViewModel> Result { get; set; } = new List<LoadViewModel>();

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public TrailerLayoutEditViewModel Model {
            get { return DataContext as TrailerLayoutEditViewModel; }
            set { DataContext = value; }
        }

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public TrailerLayoutEditDialog(
            DispatchService dispatchService, ICallService callService, IMapper mapper, TrailerViewModel cloneTrailer, IList<LoadViewModel> loads) {
            _dispatchService = dispatchService;
            _mapper = mapper;
            _callService = callService;

            InitializeComponent();

            // Replace pallets to pallets from trailer
            foreach (var load in loads) {
                load.Layout.Clear();
                foreach (var cell in cloneTrailer.Layout) {
                    foreach (var pallet in cell.Pallets) {
                        if (pallet.Load.EntityId == load.EntityId) {
                            load.Layout.Add(pallet);
                            pallet.Load = load;
                        }
                    }
                }
            }
            var addPallets = loads.Select(l => new LoadPalletViewModel(l)).ToList();
            Model = new TrailerLayoutEditViewModel() { Trailer = cloneTrailer, AddPallets = addPallets, Loads = loads };
            Model.TempPallets = new ObservableCollection<LoadPalletViewModel>();
        }

        #endregion [ .ctor ]

        private ViewModelWrapper<LoadViewModel>[] results;

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.Loads.Count == 0) {
                DialogResult = true;
                return;
            }
            results = new ViewModelWrapper<LoadViewModel>[Model.Loads.Count];
            foreach (var load in Model.Loads) {
                _dispatchService.SaveLoad(load, SaveCompleted);
            }
            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<LoadViewModel> wrapper) {
            for (int i = 0; i < results.Length; i++) {
                if (results[i] == null) {
                    results[i] = wrapper;
                    break;
                }
            }
            if (wrapper.Status == ResponseStatus.Success) {
                // NOTE: will be used in TrailerLayoutDetailsDialog
                Result.Add(wrapper.Data);
            }
            if (results[results.Length - 1] != null) {
                await base.SaveCompleted(results.OrderBy(w => w.Status).Last()); // Gets Failed result
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void EditBookLoadDialog_OnLoaded(object sender, RoutedEventArgs e) {

        }

        private void AddItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pallet = (LoadPalletViewModel)((FrameworkElement)sender).DataContext;
            var dragData = new DataObject(DragPalletSourceType.New.ToString(), pallet.Clone());
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
                var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Contains(pallet) == false) {
                        if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                            pallet.Index = (byte)cell.Index;
                            pallet.Level = (byte)cell.Pallets.Count;
                            pallet.Changed = true;

                            DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                            cell.Pallets.Add(pallet);
                        } else {
                            MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                        }
                    }
                }

                pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                        pallet.Index = (byte)cell.Index;
                        pallet.Level = (byte)cell.Pallets.Count;
                        pallet.Changed = true;

                        cell.Pallets.Add(pallet);
                        Model.TempPallets.Remove(pallet);
                        Model.Loads.First(l => l == pallet.Load).Layout.Add(pallet);
                    } else {
                        MessageBoxEx.Warning(this, $"You can not put more than {TrailerViewModel.MaxStack} pallets");
                    }
                }

                pallet = e.Data.GetData(DragPalletSourceType.New.ToString()) as LoadPalletViewModel;
                if (pallet != null && (tl.AllowDrag.Equals(true) || tl.AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (cell.Pallets.Count < TrailerViewModel.MaxStack) {
                        pallet.Index = (byte)cell.Index;
                        pallet.Level = (byte)cell.Pallets.Count;
                        pallet.Changed = true;

                        cell.Pallets.Add(pallet);
                        Model.Loads.First(l => l == pallet.Load).Layout.Add(pallet);
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
                AutoDispatch.Logic.Helpers.DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                var load = Model.Loads.First(l => l == pallet.Load);
                load.Layout.Remove(pallet);
            }

            pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                Model.TempPallets.Remove(pallet);
            }
            this.UpdateTrigger();
        }

        private void freeList_Drop(object sender, DragEventArgs e) {
            var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                DispatchHelper.FindAndRemovePalletFromTrailer(Model.Trailer, pallet);
                Model.Loads.First(l => l == pallet.Load).Layout.Remove(pallet);
                Model.TempPallets.Add(pallet);
            }
            pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
            if (pallet != null) {

            }
            pallet = e.Data.GetData(DragPalletSourceType.New.ToString()) as LoadPalletViewModel;
            if (pallet != null) {
                var targetPallet = Model.TempPallets.FirstOrDefault(p => p.Changed);
                Model.TempPallets.Add(pallet);
            }
            this.UpdateTrigger();
        }

        private void UpdateTrigger() {
            // Update all cells
            foreach (TrailerCellViewModel cell1 in Model.Trailer.Layout) {
                foreach (var pallet1 in cell1.Pallets) {
                    pallet1.UpdateTrigger();
                }
            }
        }
    }
}