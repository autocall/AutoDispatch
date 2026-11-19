using System.Windows;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Enums;
using Telerik.Windows.Controls;
using AutoCall.Logic.Contracts.Settings.Sections;
using AutoCall.Logic.Settings;
using AutoDispatch.Logic.ViewModels.Items;
using System.Windows.Input;
using AutoCall.Logic.Contracts.Enums;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls.GridView.SearchPanel;
using Telerik.Windows.Controls.GridView;
using System.Linq;

namespace AutoCall.WPF.UserControls.Boards {
    /// <summary>
    ///     Represents base board control </summary>
    public abstract class Board : RadPane {

        #region [ Protected Fields ]
        /// <summary>
        ///     Settings service </summary>
        protected SettingsService _settings;
        /// <summary>
        ///     Boards type </summary>
        protected BoardType _boardType;

        /// <summary>
        ///     Module name </summary>
        protected ModuleName _moduleName;

        #endregion [ Protected Fields ]

        #region [ Properties ]

        public virtual void SelectedCarrierCompany(ActualCompanyViewModel actualCompanyViewModel) {
            ((dynamic)DataContext).SelectedCarrierCompany = actualCompanyViewModel;
        }

        public virtual void SelectedBrokerCompany(ActualCompanyViewModel actualCompanyViewModel) {
            ((dynamic)DataContext).SelectedBrokerCompany = actualCompanyViewModel;
        }

        public virtual void SetViewType(bool viewType) { }

        /// <summary>
        ///     Gets board type </summary>
        public BoardType BoardType => _boardType;
        /// <summary>
        ///     Gets module type </summary>
        public ModuleName ModuleName => _moduleName;

        #endregion [ Properties ]

        #region [ .ctor ]

#if DEBUG
        /// <summary>
        ///     Parameterless constructor </summary>
        public Board() {
        }
#endif

        /// <summary>
        ///     Constructor </summary>
        /// <param name="boardType">Board type</param>
        /// <param name="moduleName">Module name</param>
        public Board(ModuleName moduleName, BoardType boardType, SettingsService settings)
            : base() {
            _boardType = boardType;
            _moduleName = moduleName;
            _settings = settings;

            Title = _boardType.GetDescription();

            RadDocking.SetFloatingSize(this, new Size(800, 600));
        }

        #endregion [ .ctor ]

        #region [ Public Methods ]

        // OnSessionTokenError/OnUpdateAlarmUrl V1 — события DAT (сессия, alarm url) для бордов DatPower модуля AutoCall, не перенесены

        #endregion [ Public Methods ]

        #region [ Settings ]

        /// <summary>
        ///     Apply settings </summary>
        public virtual void InitializeGrids() {
            var settings = _settings.GetBoardsSettings2();
            if (settings != null && settings.ContainsKey(BoardType)) {
                SetSettings(settings[BoardType]);
            } else {
                // default settings
                SetSettings(new BoardSettings2());
            }
        }

        public virtual void Board_OnKeyDown(object sender, KeyEventArgs e) { }

        public abstract void SetSettings(BoardSettings2 settings);

        public abstract BoardSettings2 GetSettings();

        #endregion [ Settings ]

        #region [ Zoom ]

        public abstract Slider ScaleSlider { get; }
        public abstract ScaleTransform ScaleTransform { get; }

        protected override void OnKeyDown(KeyEventArgs args) {
            base.OnKeyDown(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (args.Key == Key.Add) {
                    ScaleSlider.Value += 0.1;
                    args.Handled = true;
                }
                if (args.Key == Key.Subtract) {
                    ScaleSlider.Value -= 0.1;
                    args.Handled = true;
                }
            }
        }

        protected void ZoomInButtonClicked(object sender, RoutedEventArgs e) {
            ScaleSlider.Value += 0.1;
        }

        protected void ZoomOutButtonClicked(object sender, RoutedEventArgs e) {
            ScaleSlider.Value -= 0.1;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args) {
            base.OnPreviewMouseWheel(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl)) {
                ScaleSlider.Value += (args.Delta > 0) ? 0.1 : -0.1;
                args.Handled = true;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args) {
            base.OnPreviewMouseDown(args);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (args.MiddleButton == MouseButtonState.Pressed) {
                    RestoreScalingFactor(ScaleSlider, args);
                    args.Handled = true;
                }
            }
        }

        protected void RestoreScalingFactor(object sender, MouseButtonEventArgs args) {
            ((Slider)sender).Value = 1.0;
        }

        #endregion [ Zoom ]

        protected void grid_SearchPanelVisibilityChanged(object sender, VisibilityChangedEventArgs e) {
            if (e.NewVisibility == Visibility.Collapsed) {
                var clearSearchValue = GridViewSearchPanelCommands.ClearSearchValue as RoutedUICommand;
                clearSearchValue.Execute(null, ((RadGridView)sender).ChildrenOfType<GridViewSearchPanel>().FirstOrDefault());
            }
        }

        protected void grid_CopyingCellClipboardContent(object sender, GridViewCellClipboardEventArgs e) {
            if (((RadGridView)e.Source).CurrentCell != null && ((RadGridView)e.Source).CurrentCell.Column == e.Cell.Column) {
                e.Cancel = false;
            } else {
                e.Cancel = true;
            }
        }
    }
}