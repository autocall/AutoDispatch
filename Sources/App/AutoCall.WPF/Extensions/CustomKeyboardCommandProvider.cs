using System.Collections.Generic;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Extensions {
    public class CustomKeyboardCommandProvider : DefaultKeyboardCommandProvider {
        private GridViewDataControl dataControl;

        public CustomKeyboardCommandProvider(GridViewDataControl dataControl)
            : base(dataControl) {
            this.dataControl = dataControl;
        }

        public override IEnumerable<ICommand> ProvideCommandsForKey(Key key) {

            if (key != Key.Return)
                // use default behavior for all keys except Return
                return base.ProvideCommandsForKey(key);

            List<ICommand> commandsToExecute = new List<ICommand>();

            if (this.dataControl.CurrentCell == null)
                return commandsToExecute;

            if (this.dataControl.CurrentCell.IsInEditMode) {
                commandsToExecute.Add(RadGridViewCommands.CommitEdit);
            } else {
                commandsToExecute.Add(RadGridViewCommands.ActivateRow);
            }

            return commandsToExecute;
        }
    }
}
