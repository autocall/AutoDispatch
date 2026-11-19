using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {

    public partial class LoadDetails : Border {

        public LoadDetails() {
            InitializeComponent();
        }

        private void Photo_OnClick(object sender, RoutedEventArgs e) {
            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.EntityIdView,
                Name = load.PhotoFile
            });
        }

        private void Invoice_OnClick(object sender, RoutedEventArgs e) {
            // checks permission
            // не смог использовать DisparchCommands -> Role 
            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == false) {
                MessageBoxEx.Error("Access denied");
                return;
            }

            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.EntityIdView,
                Name = load.InvoiceFile
            });
        }

        private void Check_OnClick(object sender, RoutedEventArgs e) {
            // checks permission
            // не смог использовать DisparchCommands -> Role 
            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == false) {
                MessageBoxEx.Error("Access denied");
                return;

            }

            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.EntityIdView,
                Name = load.CheckFile
            });
        }

        private void BOL_OnClick(object sender, RoutedEventArgs e) {
            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.EntityIdView,
                Name = load.BOLFileView
            });
        }

        private void POD_OnClick(object sender, RoutedEventArgs e) {
            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.EntityIdView,
                Name = load.PODFileView
            });
        }

        private void Confirmation_OnClick(object sender, RoutedEventArgs e) {
            // checks permission
            // не смог использовать DisparchCommands -> Role 
            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == false) {
                MessageBoxEx.Error("Access denied");
                return;
            }

            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            FileLoad(new FileViewModel() {
                EntityId = (Guid)load.ConfirmationEntityIdView,
                Name = load.ConfirmationFileView
            });
        }

        private void TrailerScheme_OnClick(object sender, RoutedEventArgs e) {
            var load = (LoadViewModel)((FrameworkContentElement)sender).DataContext;
            if (load.TrailerSchemeId != null) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)load.TrailerSchemeId,
                    Name = load.TrailerSchemeFile
                });
            } else {
                _l.e($"A Load<{load.EntityId}> doesn't have TrailerSchemeId");
            }
        }

        #region open

        private void FileLoad(FileViewModel fileViewModel) {
            App.Services.GetRequiredService<DispatchService>().FileLoad(fileViewModel, FileLoadCompeleted);
            loading.Visibility = Visibility.Visible;
        }

        private async Task FileLoadCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync((Action)(() => {
                loading.Visibility = Visibility.Collapsed;
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
                            StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path }
                        };
                        process.Start();
                    } catch (Exception exception) {
                        MessageBoxEx.Error(exception.Message);
                    }
                } else {
                    MessageBoxEx.Error(wrapper.MessageError);
                }
            }));
        }

        #endregion open
    }
}
