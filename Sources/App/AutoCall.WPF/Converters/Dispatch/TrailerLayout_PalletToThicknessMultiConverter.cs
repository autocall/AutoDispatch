using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class TrailerLayout_PalletToThicknessMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double left = 0, top = 0, right = 0, bottom = 0;

            var pallet = values[0] as LoadPalletViewModel;
            var cells = values[1] as IList<TrailerCellViewModel>;

            if (pallet != null && cells != null) {
                for (int i = 0; i < cells.Count; i++) {
                    var cell = cells[i];
                    for (int j = 0; j < cell.Pallets.Count; j++) {
                        if (pallet == cell.Pallets[j]) {
                            if (pallet.Load.DivideType == DivideType.Pallets) {
                                return new Thickness(1);
                            }
                            if (pallet.Load.DivideType == DivideType.Length) {
                                if (i + 2 < cells.Count &&
                                    j < cells[i + 2].Pallets.Count &&
                                    (j == 0 || cells[i + 2].Pallets.Count == cells[i].Pallets.Count) &&
                                    cells[i + 2].Pallets[j].Load.DivideType == DivideType.Length &&
                                    cells[i + 2].Pallets[j].Load.EntityId == pallet.Load.EntityId) {
                                    bottom = 0;
                                } else {
                                    bottom = 1;
                                }
                                if (i - 2 >= 0 &&
                                    j < cells[i - 2].Pallets.Count &&
                                    (j == 0 || cells[i - 2].Pallets.Count == cells[i].Pallets.Count) &&
                                    cells[i - 2].Pallets[j].Load.DivideType == DivideType.Length &&
                                    cells[i - 2].Pallets[j].Load.EntityId == pallet.Load.EntityId) {
                                    top = 0;
                                } else {
                                    top = 1;
                                }
                                if (i % 2 == 0 && j == 0 &&
                                    cells[i + 1].Pallets.Count > 0 &&
                                    cells[i + 1].Pallets[j].Load.DivideType == DivideType.Length &&
                                    cells[i + 1].Pallets[j].Load.EntityId == pallet.Load.EntityId) {
                                    left = 1;
                                    right = 0;
                                } else {
                                    left = 1;
                                    right = 1;
                                }
                                if (i % 2 == 1 && j == 0 && cells[i].Pallets.Count == 1 &&
                                    cells[i - 1].Pallets.Count > 0 &&
                                    cells[i - 1].Pallets[j].Load.DivideType == DivideType.Length &&
                                    cells[i - 1].Pallets[j].Load.EntityId == pallet.Load.EntityId) {
                                    left = 0;
                                    right = 1;
                                } else {
                                    left = 1;
                                    right = 1;
                                }
                            }
                            if (pallet.Load.DivideType == DivideType.None) {
                                return new Thickness(1);
                            }
                        }
                    }
                }
            }

            return new Thickness(left, top, right, bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
