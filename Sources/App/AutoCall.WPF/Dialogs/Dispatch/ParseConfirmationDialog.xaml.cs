
using AutoCall.Logic.Managers;
using AutoDispatch.Logic.Converters;
using AutoCall.Logic.Settings;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class ParseConfirmationDialog : Window {

        private DispatchBoardType Board;

        public ParseConfirmationDialog(DispatchBoardType board) {
            InitializeComponent();

            dropGrid.Background = new SolidColorBrush(SystemColors.ControlColor);
            Board = board;
            StopBusy();
        }

        private void StartBusy() {
            tbError.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Visible;
            dropGrid.Opacity = 0.5;
            dropGrid.AllowDrop = false;
        }

        private void StopBusy(string error = null) {
            dropGrid.Opacity = 1;
            dropGrid.AllowDrop = true;
            progressBar.Visibility = Visibility.Collapsed;
            if (error == null) {
                tbError.Visibility = Visibility.Collapsed;
                tbError.Text = null;
            } else {
                tbError.Visibility = Visibility.Visible;
                tbError.Text = error;
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any()) {
                    await ProcessAsync(files[0]);
                } else {
                    StopBusy("Please drop a PDF file here");
                }
            }
        }

        private async void Browse_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() {
                Filter = "Image files (*.pdf) | *.pdf",
                Title = "Upload Confirmation PDF Document"
            };
            if (dialog.ShowDialog() == true) {
                await ProcessAsync(dialog.FileName);
            }
        }

        private async Task ProcessAsync(string file) {
            StartBusy();
            var data = File.ReadAllBytes(file);
            var mapper = App.Services.GetRequiredService<IMapper>();
            var manager = App.Services.GetRequiredService<MainManager>();
            var helperService = App.Services.GetRequiredService<HelperDataService>();
            var dispatchService = App.Services.GetRequiredService<DispatchService>();
            var fileName = Path.GetFileName(file);
            ExtractLoadFromPdf2Response response = await helperService.ExtractLoadFromPdfAsync(fileName, data, Board);
            if (response.IsSuccessed) {
                LoadViewModel viewModel = LoadConverter.ToVm(response.Data);
                var editLoadDialog = new EditLoadDialog(dispatchService, manager.GetCallService, mapper);
                // generates defaults
                var newViewModel = new LoadViewModel();
                newViewModel.Assign(viewModel);
                newViewModel.ConfirmationFile = fileName;
                newViewModel.Files.Add(new FileViewModel() {
                    EntityId = newViewModel.EntityId,
                    Name = fileName,
                    Data = data,
                    LazySave = true
                });
                editLoadDialog.Model = new EditBookLoadViewModel(newViewModel, DispatchBoardViewModel.Instance.ActualCompanies, null);
                editLoadDialog.Show();
                Close();
                StopBusy();
            } else {
                StopBusy(response.ErrorMessage);
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
