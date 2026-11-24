using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Win32;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class FilesControl : UserControl {

        public event Action<FileViewModel> FileDeleted;

        public ObservableCollection<FileViewModel> Model {
            get { return DataContext as ObservableCollection<FileViewModel>; }
            set { DataContext = value; }
        }

        private LoadViewModel _load;

        private Guid _entityId;

        private bool _parentIsEnabled;

        protected TextBlock _errorTextBlock;

        private DispatchService _dispatchService;

        private Action _undoAction = null;

        public FilesControl() {
            InitializeComponent();
        }

        static FilesControl() {
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(FilesControl), new UIPropertyMetadata(true, IsEnabledChanged, CoerceIsEnabled));
        }

        private new static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        private static object CoerceIsEnabled(DependencyObject d, object basevalue) {
            var parent = VisualTreeHelper.GetParent(d) as FrameworkElement;
            if (parent != null) {
                ((FilesControl)d)._parentIsEnabled = parent.IsEnabled;
                if (parent.IsEnabled == false) {
                    return true;
                }
            }
            return basevalue;
        }

        #region [ Files ]

        #region busy

        public void BusyStart() {
            loadingFiles.Visibility = Visibility.Visible;
            _errorTextBlock.Visibility = Visibility.Collapsed;
        }

        public void BusyFinish(string error = null) {
            loadingFiles.Visibility = Visibility.Collapsed;
            if (error != null) {
                _errorTextBlock.Visibility = Visibility.Visible;
                _errorTextBlock.Text = error;
            }
        }

        #endregion busy

        #region load files

        /// <summary>
        ///     Begins to load files </summary>
        public void FilesLoad(DispatchService dispatchService, TextBlock errorTextBlock, LoadViewModel load) {
            _load = load;
            _entityId = load.EntityId;
            _errorTextBlock = errorTextBlock;
            _dispatchService = dispatchService;
            FilesLoad();
        }
        /// <summary>
        ///     Begins to load files </summary>
        public void FilesLoad(DispatchService dispatchService, TextBlock errorTextBlock, Guid entityId) {
            _entityId = entityId;
            _errorTextBlock = errorTextBlock;
            _dispatchService = dispatchService;
            FilesLoad();
        }
        /// <summary>
        ///     Begins to load files </summary>
        private void FilesLoad() {
            Model?.Clear();
            BusyStart();
            _dispatchService.FilesLoad(_entityId, false, FilesLoadCompleted);
        }

        /// <summary>
        ///     Loads was completed </summary>
        private async Task FilesLoadCompleted(ViewModelWrapper<List<FileViewModel>> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                loadingFiles.Visibility = Visibility.Collapsed;
                if (wrapper.Status == ResponseStatus.Success) {
                    foreach (var fileViewModel in wrapper.Data) {
                        Model.Add(fileViewModel);
                    }
                    BusyFinish();
                } else {
                    BusyFinish(wrapper.MessageError);
                }
            });
        }

        #endregion load files

        #region save

        /// <summary>
        ///     Uploads file </summary>
        /// <param name="fileName">Full Path</param>
        /// <param name="entityId">Id of entity</param>
        public void FileSave(string fileName, Guid? entityId = null, Action undoAction = null) {
            try {
                _undoAction = undoAction;
                var viewModel = new FileViewModel();
                viewModel.EntityId = entityId ?? _entityId;
                viewModel.Name = Path.GetFileName(fileName);
                viewModel.Data = File.ReadAllBytes(fileName);

                if (FileSizeHelper.Check20MBSize(viewModel.Data)) {
                    this._dispatchService.FileSave(viewModel, FileSaveCompeleted);
                    BusyStart();
                }
            } catch (Exception) {
                undoAction?.Invoke();
                throw;
            }
        }

        /// <summary>
        ///     Uploads file </summary>
        /// <param name="fileName">Any name. The name may not match the file name. Example: 'doc.pdf'</param>
        /// <param name="data">Data of file</param>
        /// <param name="entityId">Id of entity</param>
        public void FileSave(string fileName, byte[] data, Guid? entityId = null) {
            var viewModel = new FileViewModel();
            viewModel.EntityId = entityId ?? _entityId;
            viewModel.Name = fileName;
            viewModel.Data = data;

            if (FileSizeHelper.Check20MBSize(viewModel.Data)) {
                this._dispatchService.FileSave(viewModel, FileSaveCompeleted);
                BusyStart();
            }
        }

        private async Task FileSaveCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                Model.Clear();
                if (wrapper.Status == ResponseStatus.Success) {
                    _undoAction = null;
                    FilesLoad();
                    BusyFinish();
                } else {
                    _undoAction?.Invoke();
                    BusyFinish(wrapper.MessageError);
                    MessageBoxEx.Error(wrapper.MessageError);
                }
            });
        }

        #endregion save

        #region open

        public void FileLoad(FileViewModel fileViewModel) {
            BusyStart();
            _dispatchService.FileLoad(fileViewModel, FileLoadCompleted);
        }

        private async Task FileLoadCompleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                if (wrapper.Status == ResponseStatus.Success) {
                    try {
                        var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                        if (Directory.Exists(path) == false) {
                            Directory.CreateDirectory(path);
                        }
                        var fullPath = Path.Combine(path, wrapper.Data.Name);
                        File.WriteAllBytes(fullPath, wrapper.Data.Data);
                        var process = new Process {
                            EnableRaisingEvents = true,
                            StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path, UseShellExecute = true }
                        };
                        process.Start();
                        if (_parentIsEnabled) {
                            process.Exited += async (sender, e) => {
                                if (MessageBox.Show("Do you want to save the file?", "Saving", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                    await this.Dispatcher.InvokeAsync(() => {
                                        FileSave(fullPath);
                                    });
                                }
                            };
                        }
                        BusyFinish();
                    } catch (Exception exception) {
                        BusyFinish(exception.Message);
                    }
                } else {
                    BusyFinish(wrapper.MessageError);
                }
            });
        }

        #endregion open

        #region copy

        private void FileCopy(FileViewModel fileViewModel) {
            BusyStart();
            _dispatchService.FileLoad(fileViewModel, FileCopyCompeleted);
        }

        private async Task FileCopyCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                if (wrapper.Status == ResponseStatus.Success) {
                    try {
                        var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                        if (Directory.Exists(path) == false) {
                            Directory.CreateDirectory(path);
                        }
                        var fullPath = Path.Combine(path, wrapper.Data.Name);
                        File.WriteAllBytes(fullPath, wrapper.Data.Data);
                        var process = new Process {
                            EnableRaisingEvents = true,
                            StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path, UseShellExecute = true }
                        };
                        var paths = new StringCollection { fullPath };
                        Clipboard.SetFileDropList(paths);
                        BusyFinish();
                    } catch (Exception exception) {
                        BusyFinish(exception.Message);
                    }
                } else {
                    BusyFinish(wrapper.MessageError);
                }
            });
        }

        #endregion copy

        #region delete

        public void FileDelete(FileViewModel fileViewModel) {
            BusyStart();
            _dispatchService.FileDelete(fileViewModel, FileDeleteCompeleted);
        }

        private async Task FileDeleteCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync(() => {
                if (wrapper.Status == ResponseStatus.Success) {
                    FilesLoad();
                    BusyFinish();
                    FileDeleted?.Invoke(wrapper.Data);
                } else {
                    BusyFinish(wrapper.MessageError);
                }
            });
        }

        #endregion delete

        private void FileDrop(object sender, DragEventArgs e) {
            var dataObject = e.Data as DataObject;
            if (dataObject != null && dataObject.ContainsFileDropList()) {
                foreach (string fileName in dataObject.GetFileDropList()) {
                    FileSave(fileName);
                }
            }
        }

        private void FileDoubleClick(object sender, MouseButtonEventArgs e) {
            if (filesListBox.SelectedItem != null) {
                FileLoad((FileViewModel)filesListBox.SelectedItem);
            }
        }

        private void filesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            // checks not scroll 
            if (e.OriginalSource is ScrollViewer == false) {
                return;
            }
            // handler for TextBlock
            if (sender is TextBlock && e.ChangedButton == MouseButton.Left && e.ClickCount != 2) {
                Debug.WriteLine("filesListBox_MouseDoubleClick " + e.ClickCount);
                return;
            }
            // handler for ListBox
            if (filesListBox.SelectedItems.Count == 0) {
                var dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == true) {
                    foreach (var fileName in dialog.FileNames) {
                        FileSave(fileName);
                    }
                }
            }
        }

        private void filesListBox_MouseDown(object sender, MouseButtonEventArgs e) {
            var source = e.Source as FrameworkElement;
            if (source != null && source.DataContext is FileViewModel == false) {
                filesListBox.SelectedItem = null;
            }
        }

        private void CommandDeleteFileExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (filesListBox.SelectedItem != null) {
                if (MessageBox.Show(
                        $"Are you sure you want to delete '{((FileViewModel)filesListBox.SelectedItem).Name}'?", "",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    FileDelete((FileViewModel)filesListBox.SelectedItem);
            }
        }

        private void CommandCopyFileExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (filesListBox.SelectedItem != null) {
                FileCopy((FileViewModel)filesListBox.SelectedItem);
            }
        }

        private void CommandOpenFileExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (filesListBox.SelectedItem != null) {
                this.FileLoad((FileViewModel)filesListBox.SelectedItem);
            }
        }

        private void CommandBindingCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            // TODO implement statuses
            //if (this.Model.Status == )
            //    e.CanExecute = false;
            //else
            e.CanExecute = true;
        }

        #endregion [ Files ]
    }
}
