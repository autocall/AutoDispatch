using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Loggers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace AutoDispatch.Logic.ViewModels {
    public class Command : ICommand {

        #region [ _fields ]

        private Action<Wrapper1LoadViewModel> SingleExecuteAction = null;
        private Predicate<Wrapper1LoadViewModel> SingleCanExecuteAction = null;
        private Action<List<Wrapper1LoadViewModel>> MultiExecuteAction = null;
        private Predicate<List<Wrapper1LoadViewModel>> MultiCanExecuteAction = null;

        private RoleName MinRole;
        private RoleName[] ExtraRoles;
        private RoleName MyRole;

        private DispatchBoardType? DispatchBoardType;

        #endregion [ _fields ]

        /// <summary>
        ///     is use in CanExecute </summary>
        public virtual bool CheckBoard { get; set; }

        #region [ .ctor ]

        public Command(
            DispatchBoardType? dispatchBoardType,
            RoleName myRole, RoleName minRole,
            Action<Wrapper1LoadViewModel> singleExecute,
            Predicate<Wrapper1LoadViewModel> singleCanExecute = null,
            Action<List<Wrapper1LoadViewModel>> multiExecute = null,
            Predicate<List<Wrapper1LoadViewModel>> multiCanExecute = null,
            RoleName[] extraRoles = null,
            bool checkBoard = true) {
            this.DispatchBoardType = dispatchBoardType;
            this.MinRole = minRole;
            this.MyRole = myRole;
            this.SingleExecuteAction = singleExecute;
            this.SingleCanExecuteAction = singleCanExecute;
            this.MultiExecuteAction = multiExecute;
            this.MultiCanExecuteAction = multiCanExecute;
            this.ExtraRoles = extraRoles;
            this.CheckBoard = checkBoard;
        }

        #endregion [ .ctor ]

        #region [ Public ]

        public bool CanExecute(object parameter) {
            if (RolesExtension.CheckPermission(MyRole, MinRole, ExtraRoles) == false) return false;
            if (parameter == null) return false;
            if (Object.Equals(parameter, "null")) return true;

            if (parameter is IList<object>) {
                var list = ((IList<object>)parameter).Cast<Wrapper1LoadViewModel>().ToList();
                if (list.Count == 0) return false;
                //if (list.All(e => e.Load.BoardType == DispatchBoardType) == false) return false;
                if (list.Count == 1 && SingleCanExecuteAction != null) {
                    return SingleCanExecuteAction(list[0]);
                } else
                if (MultiCanExecuteAction != null) {
                    return MultiCanExecuteAction(list);
                } else
                    return list.Count == 1;
            }

            if (parameter is Wrapper1LoadViewModel) {
                // check by null because history items have different Types
                // refactoring DispatchCommands.cs "new Command(this.dc<?>."
                if (this.CheckBoard && 
                    this.DispatchBoardType != null && 
                    ((Wrapper1LoadViewModel)parameter).Load.BoardType != this.DispatchBoardType) 
                    return false;
                if (SingleCanExecuteAction != null) {
                    return SingleCanExecuteAction((Wrapper1LoadViewModel)parameter);
                } else {
                    return true;
                }
            }
            return false;
        }

        public void Execute(object parameter) {
            if (RolesExtension.CheckPermission(MyRole, MinRole, ExtraRoles) == false) return;

            if (parameter == null) return;
            if (Object.Equals(parameter, "null")) {
                SingleExecuteAction(null);
            }
            if (parameter is IList<object>) {
                var list = ((IList<object>)parameter).Cast<Wrapper1LoadViewModel>().ToList();
                if (list.Count == 0) return;
                if (list.All(e => e.Load.BoardType == DispatchBoardType) == false) return;
                if (list.Count == 1 && SingleExecuteAction != null) {
                    SingleExecuteAction(list[0]);
                } else
                if (MultiExecuteAction != null) {
                    MultiExecuteAction(list);
                }
            }

            if (parameter is Wrapper1LoadViewModel) {
                if (this.CheckBoard && 
                    this.DispatchBoardType != null && 
                    ((Wrapper1LoadViewModel)parameter).Load.BoardType != this.DispatchBoardType) 
                    return;
                if (SingleExecuteAction != null) {
                    SingleExecuteAction((Wrapper1LoadViewModel)parameter);
                }
            }
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion [ Public ]
    }
}