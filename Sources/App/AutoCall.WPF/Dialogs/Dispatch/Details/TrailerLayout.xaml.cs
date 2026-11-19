using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.WPF.Enums;
using AutoCall.WPF.Helpers;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class TrailerLayout : UserControl {
        public bool AllowReorder { get; set; }

        public object AllowDrag {
            get { return GetValue(AllowDragProperty); }
            set { SetValue(AllowDragProperty, value); }
        }

        public static readonly DependencyProperty AllowDragProperty =
            DependencyProperty.Register("AllowDrag", typeof(object), typeof(TrailerLayout), new FrameworkPropertyMetadata() { DefaultValue = false });

        public TrailerLayout() {
            InitializeComponent();
        }

        private void template_CheckDropTarget(object sender, DragEventArgs e) {
            var pallet = e.Data.GetData(DragPalletSourceType.Trailer.ToString()) as LoadPalletViewModel;
            var ui = UIElementHelper.FindVisualParent<ContentControl>(e.OriginalSource as UIElement);
            var cell = ui?.DataContext as TrailerCellViewModel;
            if (cell != null) {
                if (pallet != null && (AllowDrag.Equals(true) || AllowDrag.Equals(pallet.Load.EntityId))) {
                    if (AllowReorder) {
                        if (cell.Pallets.Contains(pallet) == false) {
                            e.Effects = DragDropEffects.Move;
                        }
                    }
                }

                pallet = e.Data.GetData(DragPalletSourceType.Free.ToString()) as LoadPalletViewModel;
                if (pallet != null && (AllowDrag.Equals(true) || AllowDrag.Equals(pallet.Load.EntityId))) {
                    e.Effects = DragDropEffects.Move;
                }

                pallet = e.Data.GetData(DragPalletSourceType.New.ToString()) as LoadPalletViewModel;
                if (pallet != null && (AllowDrag.Equals(true) || AllowDrag.Equals(pallet.Load.EntityId))) {
                    e.Effects = DragDropEffects.Move;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void template_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pallet = (LoadPalletViewModel)((FrameworkElement)sender).DataContext;
            if (pallet != null && (AllowDrag.Equals(true) || AllowDrag.Equals(pallet.Load.EntityId))) {
                var dragData = new DataObject(DragPalletSourceType.Trailer.ToString(), pallet);
                DragDropEffects finalDropEffect = DragDrop.DoDragDrop((DependencyObject)e.OriginalSource, dragData, DragDropEffects.Move);
            }
        }

        private void template_MouseEnter(object sender, MouseEventArgs e) {
            var pallet = (LoadPalletViewModel)((FrameworkElement)sender).DataContext;
            if (pallet != null && (AllowDrag.Equals(true) || AllowDrag.Equals(pallet.Load.EntityId))) {
                Cursor = Cursors.Hand;
            }
        }

        private void template_MouseLeave(object sender, MouseEventArgs e) {
            Cursor = Cursors.Arrow;
        }
    }
}
