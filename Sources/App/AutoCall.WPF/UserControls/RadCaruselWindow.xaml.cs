using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace AutoCall.WPF.UserControls {
    public partial class RadCaruselWindow : Window {
        /// <summary>
        ///     Gets or sets view model </summary>
        public ObservableCollection<FileViewModel> ItemsSource {
            get { return DataContext as ObservableCollection<FileViewModel>; }
            set { DataContext = value; }
        }

        private DispatchService _dispatchService;

        public RadCaruselWindow(DispatchService dispatchService) {
            this._dispatchService = dispatchService;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            if (this.ItemsSource.Any()) {
                await Task.Delay(TimeSpan.FromSeconds(0.2));
                radCarousel.BringDataItemIntoView(this.ItemsSource[0]);
            }
        }

        public bool SetImageItemsSource(IEnumerable<FileViewModel> files) {
            this.ItemsSource = new ObservableCollection<FileViewModel>();
            foreach (var file in files) {
                if (ImageHelper.IsImage(file.Name) == false) {
                    continue;
                }
                var clone = (FileViewModel)file.Clone();
                clone.Preview = true;
                this.ItemsSource.Add(clone);
            }
            return this.ItemsSource.Any();
        }

        private void pageDownButton_Click(object sender, RoutedEventArgs e) {
            this.radCarousel.FindCarouselPanel().PageDown();
        }
        private void pageLeftButton_Click(object sender, RoutedEventArgs e) {
            this.radCarousel.FindCarouselPanel().MoveBy(-1);
        }
        private void pageRightButton_Click(object sender, RoutedEventArgs e) {
            this.radCarousel.FindCarouselPanel().MoveBy(1);
        }
        private void pageUpButton_Click(object sender, RoutedEventArgs e) {
            this.radCarousel.FindCarouselPanel().PageUp();
        }

        private void radCarousel_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (((FrameworkElement)e.OriginalSource).DataContext is FileViewModel vm) {
                if (vm.Preview == true) {
                    vm.Preview = false;
                    _dispatchService.FileLoad(vm, FileLoadCompleted);
                } else {
                    OpenFile(vm);
                }
            }
        }

        private async Task FileLoadCompleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                if (wrapper.Status == ResponseStatus.Success) {
                    foreach (var file in ItemsSource) {
                        if (file.Name == wrapper.Data.Name) {
                            file.Data = wrapper.Data.Data;
                            file.Preview = wrapper.Data.Preview;
                            file.DataImage = ImageHelper.ToImage(file.Data);
                            OpenFile(file);
                            break;
                        }
                    }
                }
            });
        }

        private void OpenFile(FileViewModel fileViewModel) {
            var path = Path.Combine(Path.GetTempPath(), fileViewModel.EntityId.ToString());
            if (Directory.Exists(path) == false) {
                Directory.CreateDirectory(path);
            }
            var fullPath = Path.Combine(path, fileViewModel.Name);
            File.WriteAllBytes(fullPath, fileViewModel.Data);
            var process = new Process {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path }
            };
            process.Start();
        }
    }
}
