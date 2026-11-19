using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using AutoCall.Common;
using AutoCall.Logic.ViewModels;

namespace AutoCall.WPF.UserControls {
    /// <summary>
    ///     Interaction logic for GMapControl.xaml </summary>
    public partial class GMapControl : UserControl, IDisposable {

        /// <summary>
        ///     Constructor </summary>
        public GMapControl() {
            InitializeComponent();
            browser.Focus();
        }

        public void Dispose() {
            browser.Dispose();
        }

        /// <summary>
        ///     Calculates route </summary>
        public void CalculateRoute(GMapViewModel points) {
            // Get path to template file
            string templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"Templates\GoogleMapsTemplate.html");
            if (!File.Exists(templatePath))
                return;

            // Get template content
            string templateContent = File.ReadAllText(templatePath);
            // Pass necessary coordinates for route
            string htmlContent = templateContent
                .Replace("[InitialOriginX]"     , points.InitialOriginCoordinates     .Latitude .ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[InitialOriginY]"     , points.InitialOriginCoordinates     .Longitude.ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[OriginX]"            , points.OriginCoordinates            .Latitude .ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[OriginY]"            , points.OriginCoordinates            .Longitude.ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[DestinationX]"       , points.DestinationCoordinates       .Latitude .ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[DestinationY]"       , points.DestinationCoordinates       .Longitude.ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[InitialDestinationX]", points.InitialDestinationCoordinates.Latitude .ToString(CultureInfo.CreateSpecificCulture("en-US")))
                .Replace("[InitialDestinationY]", points.InitialDestinationCoordinates.Longitude.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            
            // Show map in browser control
            browser.NavigateToString(htmlContent);
        }
    }
}