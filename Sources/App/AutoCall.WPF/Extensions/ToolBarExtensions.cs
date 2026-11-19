using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AutoCall.WPF.UserControls;
using WpfAnimatedGif;
using System.Windows.Media;

namespace AutoCall.WPF.Extensions {
    /// <summary>
    ///     Represents extension methods for ToolBar control </summary>
    public static class ToolBarExtensions {

        #region [ Public Methods ]

        public static Button CreateButton(Uri imgUri, string content, Func<object, object> findResource) {
            var b = new Button() {
                Style = (Style)findResource(ToolBar.ButtonStyleKey)
            };
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = imgUri;
            bi.EndInit();
            var img = new AutoGreyableImage() {
                Source = bi,
                Style = (Style)findResource("BoardButtonImageStyle")
            };
            var tb = new TextBlock() {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3),
                Text = content
            };
            var sp = new StackPanel() {
                Orientation = Orientation.Horizontal
            };
            sp.Children.Add(img);
            sp.Children.Add(tb);
            b.Content = sp;
            return b;
        }

        public static Button CreateGifButton(Uri imgUri, string content, Func<object, object> findResource) {
            var b = new Button() {
                Style = (Style)findResource(ToolBar.ButtonStyleKey)
            };
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = imgUri;
            bi.EndInit();
            var img = new AutoGreyableImage() {
                Style = (Style)findResource("BoardButtonImageStyle"),
                Stretch = Stretch.None
            };
            ImageBehavior.SetAnimatedSource(img, bi);
            var tb = new TextBlock() {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3),
                Text = content
            };
            var sp = new StackPanel() {
                Orientation = Orientation.Horizontal
            };
            sp.Children.Add(img);
            sp.Children.Add(tb);
            b.Content = sp;
            return b;
        }

        public static Button CreateButton(string content, Func<object, object> findResource) {
            var b = new Button() {
                Style = (Style)findResource(ToolBar.ButtonStyleKey)
            };
            var tb = new TextBlock() {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3),
                Text = content
            };
            var sp = new StackPanel() {
                Orientation = Orientation.Horizontal
            };
            sp.Children.Add(tb);
            b.Content = sp;
            return b;
        }

        #endregion [ Public Methods ]
    }
}