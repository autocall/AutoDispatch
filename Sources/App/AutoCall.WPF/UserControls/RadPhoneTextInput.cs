using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using System;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.MaskedInput;

namespace AutoCall.WPF.UserControls {
    public class RadPhoneTextInput : RadMaskedTextInput {

        private const string PhoneMask11 = "+# (###) ###-####";
        private const string PhoneMask10 = "(###) ###-####";

        public RadPhoneTextInput() {
            DataContextChanged += (sender, e) => { SetMask((DataContext as ContactBasePhoneViewModel)?.Phone); };
        }

        private void SetMask(string phone) {
            if (phone?.Length > 10) {
                this.Mask = PhoneMask11;
            } else {
                this.Mask = PhoneMask10;
            }
        }

        protected override bool HandlePasteOverride(object value, out object returnString) {
            if(value is string) {
                value = StringExtension.OnlyNumbers((string)value);
            }
            return base.HandlePasteOverride(value, out returnString);
        }

        protected override void OnLoadedOverride(object sender, RoutedEventArgs e) {
            SetMask(this.Value as string);
            InputBehavior = InputBehavior.Insert;
            base.OnLoadedOverride(sender, e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (this.Mask == PhoneMask11) {
                var value = this.Value as string;
                if (String.IsNullOrEmpty(value) == false) {
                    if (value.Length <= 10) {
                        var oldEditableTextLength = EditableTextLength;
                        this.Mask = PhoneMask10;
                        SelectionStart -= oldEditableTextLength - EditableTextLength - 1;
                    }
                }
            }
            base.OnKeyUp(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            if (this.Mask == PhoneMask10) {
                if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)) {
                    var value = this.Value as string;
                    if (String.IsNullOrEmpty(value) == false) {
                        if (value.Length >= 10) {
                            var oldEditableTextLength = EditableTextLength;
                            this.Mask = PhoneMask11;
                            if (SelectionStart != EditableTextStart) {
                                SelectionStart += EditableTextLength - oldEditableTextLength - 1;
                            }
                        }
                    }
                    return;
                }
            }
            base.OnKeyDown(e);
        }
    }
}
