using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Win32;

namespace AutoCall.WPF.Dialogs {
    public abstract class FileBaseDialog : BaseDispatchDialog {

        protected abstract TextBox TbFile { get; }

        protected DispatchService _service;

        protected byte[] FileData;

        // without extension
        protected abstract string FileName { get; }

        protected FileBaseDialog(DispatchService service) {
            _service = service;
        }

        protected void browse_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true) {
                FileData = File.ReadAllBytes(dialog.FileName); // note: possible throw exception
                TbFile.Text = $"{this.FileName}{Path.GetExtension(dialog.FileName)}";
            }
        }

        protected void scan_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ScannerDialog();
            var data = dlg.ShowDialog() == true ? dlg.GeneratePdf() : null;
            if (data != null) {
                FileData = data;
                TbFile.Text = $"{this.FileName}.pdf";
            }
        }

        protected async Task SaveLoad_Completed(ViewModelWrapper<LoadViewModel> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveLoad_Completed(wrapper));
                return;
            }
            DataContext = wrapper.Data;
            await SaveCompleted(wrapper);
        }

        protected void file_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Any()) {
                    FileData = File.ReadAllBytes(files[0]); // note: possible throw exception
                    TbFile.Text = files[0];
                    TbFile.CaretIndex = TbFile.Text.Length;
                    var rect = TbFile.GetRectFromCharacterIndex(TbFile.CaretIndex);
                    TbFile.ScrollToHorizontalOffset(rect.Right);
                }
            }
        }

        protected void file_DragEnter(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            e.Handled = !e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        protected void file_PreviewDragOver(object sender, DragEventArgs e) {
            e.Handled = true;
        }
    }
}
