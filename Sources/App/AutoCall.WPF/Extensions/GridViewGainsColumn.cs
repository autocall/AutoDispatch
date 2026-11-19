using AutoCall.WPF.UserControls;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Extensions {
    public class GridViewGainsColumn : SumDataColumn {

        public GridViewGainsColumn() {
            this.IsReadOnly = true;
        }

        public GridViewGainsColumn(RadGridView gridView, bool linkedLoad, int index, GainType gain) {
            var prefixBinding = linkedLoad == true ? "LinkedWrapper." : String.Empty;
            this.Tag = gain;
            this.CellStyle = new Style(typeof(GridViewCell), (Style)gridView.Resources[typeof(GridViewCell)]);
            this.SetColumnStyle1($"{prefixBinding}Load.GainsView[{index}].Paid", $"{prefixBinding}Load.Parent1");
            this.CellStyle.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding($"{prefixBinding}Load.GainsView[{index}].PaidDate") {
                Converter = (IValueConverter)App.Current.Resources["GainPaidDateToStringConverter"]
            }));

            // Grid.Tag поможет определить GainViewModel по sender-у
            // Button.Tag мужен, когда проверяем на модифицирован cell или нет
            var stringTempate = $@"
<DataTemplate>
    <Grid DataContext=""{{Binding {prefixBinding.TrimEnd('.')}}}"">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width=""*""/>
            <ColumnDefinition Width=""Auto""/>
        </Grid.ColumnDefinitions>
        <TextBox Grid.Column=""0"" Text=""{{Binding Load.GainsView[{index}].Cost, UpdateSourceTrigger=PropertyChanged, TargetNullValue='', StringFormat=C2}}""/>
        <Button Grid.Column=""1""
            Click =""{{x:Static boards:DispatchBoardBase.Paid_OnClick}}"" 
            Foreground=""{{Binding Load.GainsView[{index}].Paid, Converter={{StaticResource BoolToBlackGrayConverter}}}}""
             Tag=""{{Binding Load.GainsView[{index}].Type, Mode=OneTime}}""
            Content=""Paid"" Padding=""2,0"" Margin=""-1,0,0,0""/>
    </Grid>
</DataTemplate>";
            var context = new ParserContext();
            context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("telerik", "http://schemas.telerik.com/2008/xaml/presentation");
            context.XmlnsDictionary.Add("boards", "clr-namespace:AutoCall.WPF.UserControls.Boards;assembly=AutoCall.WPF");
            var template = (DataTemplate)XamlReader.Parse(stringTempate, context);
            this.CellEditTemplate = template;
        }
    }
}
