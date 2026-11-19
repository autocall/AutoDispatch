using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditDriverDialog : BaseDispatchDialog {

        #region [ Base ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ Base ]

        #region [ _fields ]

        private DispatchService _service;

        #endregion [ _fields ]

        #region [ View Model ]

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DriverViewModel Model {
            get { return DataContext as DriverViewModel; }
            set { DataContext = value; }
        }

        private DriverViewModel _clone;

        #endregion [ View Model ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public EditDriverDialog(DispatchService service, DriverViewModel driver, ObservableCollection<TruckViewModel> trucks) {
            _service = service;

            InitializeComponent();

            if (driver != null) {
                Model = driver;
                Title = "Edit Driver";
            } else {
                Model = new DriverViewModel();
                Title = "Add Driver";
            }
            _clone = (DriverViewModel)Model.Clone();

            cbTrucks.ItemsSource = trucks;
        }

        #endregion [ .ctor ]

        #region [ Event handles ]

        private void EditDriverDialog_Loaded(object sender, RoutedEventArgs e) {
            filesControl.FilesLoad(_service, tbError, Model.EntityId);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Model.Name)) {
                MessageBoxEx.Error(this, "Please enter a Name");
                return;
            }

            if (StringExtension.IsValidEmail(Model.Email) == false) {
                MessageBoxEx.Error(this, "Please enter a valid Email");
                return;
            }

            if (Model.MVR_File != null && Model.MVR_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'MVR'");
                return;
            }
            if (Model.MEDICAL_CARD_File != null && Model.MEDICAL_CARD_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'MEDICAL CARD'");
                return;
            }
            if (Model.CDL_File != null && Model.CDL_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'CDL'");
                return;
            }
            if (Model.ACH_File != null && Model.ACH_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'ACH'");
                return;
            }
            if (Model.W9_File != null && Model.W9_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'W9'");
                return;
            }

            _service.SaveDriver(Model, (w) => this.SaveCompleted(w));
            base.BusyStart();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        #region [ Files ]

        private void OpenTHE_APPLICATION_OF_EMPLOYMENT_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.THE_APPLICATION_OF_EMPLOYMENT_File
            });
        }

        private void BrowseTHE_APPLICATION_OF_EMPLOYMENT_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach THE APPLICATION OF EMPLOYMENT" };
            if (dialog.ShowDialog() == true) {
                Model.THE_APPLICATION_OF_EMPLOYMENT_File = $"THE_APPLICATION_OF_EMPLOYMENT_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.THE_APPLICATION_OF_EMPLOYMENT_File, File.ReadAllBytes(dialog.FileName));
            }
        }

        private void OpenMVR_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.MVR_File
            });
        }

        private void BrowseMVR_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach MVR" };
            if (dialog.ShowDialog() == true) {
                Model.MVR_File = $"MVR_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.MVR_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.MVR_Expiration == _clone.MVR_Expiration) {
                    Model.MVR_Expiration = null;
                }
            }
        }

        private void OpenMEDICAL_CARD_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.MEDICAL_CARD_File
            });
        }

        private void BrowseMEDICAL_CARD_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach MEDICAL CARD" };
            if (dialog.ShowDialog() == true) {
                Model.MEDICAL_CARD_File = $"MEDICAL_CARD_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.MEDICAL_CARD_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.MEDICAL_CARD_Expiration == _clone.MEDICAL_CARD_Expiration) {
                    Model.MEDICAL_CARD_Expiration = null;
                }
            }
        }

        private void OpenCDL_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.CDL_File
            });
        }

        private void BrowseCDL_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach CDL" };
            if (dialog.ShowDialog() == true) {
                Model.CDL_File = $"CDL_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.CDL_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.CDL_Expiration == _clone.CDL_Expiration) {
                    Model.CDL_Expiration = null;
                }
            }
        }

        private void OpenDOT_INSPECTIONS_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.DOT_INSPECTIONS_File
            });
        }

        private void BrowseDOT_INSPECTIONS_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach DOT INSPECTIONS" };
            if (dialog.ShowDialog() == true) {
                Model.DOT_INSPECTIONS_File = $"DOT_INSPECTIONS_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.DOT_INSPECTIONS_File, File.ReadAllBytes(dialog.FileName));
            }
        }

        private void OpenBACKGROUND_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.BACKGROUND_File
            });
        }

        private void BrowseBACKGROUND_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach BACKGROUND" };
            if (dialog.ShowDialog() == true) {
                Model.BACKGROUND_File = $"BACKGROUND_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.BACKGROUND_File, File.ReadAllBytes(dialog.FileName));
            }
        }

        private void OpenDRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.DRUG_TEST_File
            });
        }

        private void BrowseDRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach DRUG_TEST" };
            if (dialog.ShowDialog() == true) {
                Model.DRUG_TEST_File = $"DRUG_TEST_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.DRUG_TEST_File, File.ReadAllBytes(dialog.FileName));
            }
        }

        private void OpenRANDOM_DRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.RANDOM_DRUG_TEST_File
            });
        }

        private void BrowseRANDOM_DRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach RANDOM_DRUG_TEST" };
            if (dialog.ShowDialog() == true) {
                Model.RANDOM_DRUG_TEST_File = $"RANDOM_DRUG_TEST_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.RANDOM_DRUG_TEST_File, File.ReadAllBytes(dialog.FileName));
            }
        }

        private void OpenACH_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.ACH_File
            });
        }

        private void BrowseACH_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach ACH" };
            if (dialog.ShowDialog() == true) {
                Model.ACH_File = $"ACH_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.ACH_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.ACH_Expiration == _clone.ACH_Expiration) {
                    Model.ACH_Expiration = null;
                }
            }
        }

        private void OpenW9_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.W9_File
            });
        }

        private void BrowseW9_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach W9" };
            if (dialog.ShowDialog() == true) {
                Model.W9_File = $"W9_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.W9_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.W9_Expiration == _clone.W9_Expiration) {
                    Model.W9_Expiration = null;
                }
            }
        }

        #endregion [ Files ]

        #endregion [ Event handles ]
    }
}