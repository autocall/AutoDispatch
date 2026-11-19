using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Helpers {
    /// <summary>
    ///     Пункт «Copy» контекстного меню стиля valueBindableTextBlock (App.xaml). В V1 — статический обработчик базового класса
    ///     карточки записи лоадборда RecordBaseDetailsDialog (модуль AutoCall, не перенесён); код обработчика тот же </summary>
    public static class CopyMenuHelper {

        public static RoutedEventHandler Copy_OnClick => _copy_OnClick;
        private static void _copy_OnClick(object sender, RoutedEventArgs e) {
            // NOTES: PlacementTarget показал предыдущий текст
            var ui = (TextBlock)(((RadContextMenu)((RadMenuItem)sender).Menu).UIElement ??
                ((RadContextMenu)((RadMenuItem)sender).Menu).PlacementTarget);
            var text = ui.RecursiveSearchText();
            if (String.IsNullOrEmpty(text) == false) {
                Clipboard.SetDataObject(text);
            }
        }
    }
}
