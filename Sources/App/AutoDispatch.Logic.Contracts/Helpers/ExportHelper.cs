using System;
using System.Collections.Generic;
using System.IO;
using AutoCall.Common;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace AutoDispatch.Logic.Helpers {

    public class ExportExcelColumn {
        public string UniqueName { get; set; }
        public string BindingPath { get; set; }
        public string StringFormat { get; set; }
        public double Width { get; set; }
        public string Header { get; set; }
        public string Group { get; set; }
    }

    public static class ExportHelper {

        public static void aa() {
            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets.Add();
            IWorkbookFormatProvider formatProvider = new XlsxFormatProvider();
            using (Stream output = new FileStream("", FileMode.OpenOrCreate)) {
                formatProvider.Export(workbook, output);
            }
        }

        public static void CreateFile(string fileName, DispatchBoardType dispatchBoardType, List<Wrapper1LoadViewModel> rows, List<ExportExcelColumn> columns) {

            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets.Add();

            var columnCursor = 0;
            string lastGroupName = null;
            //rows
            for (int i = 0; i < columns.Count; i++) {
                if (columns[i].UniqueName == "Row" || columns[i].BindingPath == "ActualCompany.LogoSmall") {
                    columnCursor--;
                    continue;
                }

                if (lastGroupName != columns[i].Group) {
                    worksheet.Cells[0 + 0, columnCursor + i].SetValue(columns[i].Group);
                } else {
                    CellSelection periodCells = worksheet.Cells[0, columnCursor + i - 1, 0 + 0, columnCursor + i];
                    periodCells.Merge();
                }
                lastGroupName = columns[i].Group;

                var columnSelection = worksheet.Columns[columnCursor + i, columnCursor + i + 1];
                columnSelection.SetWidth(new ColumnWidth(columns[i].Width * 1.5, true));
                worksheet.Cells[0 + 1, columnCursor + i].SetValue(columns[i].Header);

                for (int j = 0; j < rows.Count; j++) {
                    object value;
                    string format = columns[i].StringFormat;
                    try {
                        value = ReflectionHelper.GetPropValue(rows[j], columns[i].BindingPath);
                    } catch (Exception) {
                        continue;
                    }
                    if (value == null)
                        continue;
                    string str;

                    if (value is short || value is int || value is float || value is double || value is decimal) {
                        worksheet.Cells[j + 2, columnCursor + i].SetValue(Convert.ToDouble(value));
                        if (format == "N2" || format == "C2") {
                            worksheet.Cells[j + 2, columnCursor + i].SetFormat(new CellValueFormat("0.00"));
                        } else if (format != null) {
                            _l.w($"Undefined format {format}");
                        }
                        worksheet.Cells[j + 2, columnCursor + i].SetHorizontalAlignment(RadHorizontalAlignment.Left);
                    } else if (value is TimeSpan && format == "%d") {
                        worksheet.Cells[j + 2, columnCursor + i].SetValue((int)((TimeSpan)value).TotalDays);
                    } else {
                        if (value is decimal) {
                            str = ((decimal)value).ToString(format);
                        } else if (value is DateTime) {
                            str = ((DateTime)value).ToString(format);
                        } else if (value is System.Windows.Media.Imaging.BitmapImage) {
                            continue;
                        } else {
                            str = value.ToString();
                        }

                        if (columns[i].UniqueName == "Load.InvoiceNumber") {
                            if (rows[j].Load.InvoiceDate != null) {
                                str = $"{value} {rows[j].Load.InvoiceDate.Value:d}";
                            }
                        }
                        worksheet.Cells[j + 2, columnCursor + i].SetValueAsText(str);
                        worksheet.Cells[j + 2, columnCursor + i].SetHorizontalAlignment(RadHorizontalAlignment.Left);
                    }
                }
            }

            IWorkbookFormatProvider formatProvider = new XlsxFormatProvider();
            using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate)) {
                formatProvider.Export(workbook, output);
            }
        }

        public static void CreateFile(string fileName, List<DatCompanyViewModel> rows, List<ExportExcelColumn> columns) {

            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets.Add();

            //rows
            for (int i = 0; i < columns.Count; i++) {
                if (columns[i].BindingPath == "ActualCompanyId") {
                    continue;
                }
                for (int j = 0; j < rows.Count; j++) {
                    object value;
                    string format = columns[i].StringFormat;
                    try {
                        value = ReflectionHelper.GetPropValue(rows[j], columns[i].BindingPath);
                    } catch (Exception) {
                        continue;
                    }
                    if (value == null)
                        continue;
                    string str;

                    if (value is short || value is int || value is float || value is double || value is decimal) {
                        worksheet.Cells[j, i].SetValue(Convert.ToDouble(value));
                        if (format == "N2" || format == "C2") {
                            worksheet.Cells[j, i].SetFormat(new CellValueFormat("0.00"));
                        } else if (format != null) {
                            _l.w($"Undefined format {format}");
                        }
                        worksheet.Cells[j, i].SetHorizontalAlignment(RadHorizontalAlignment.Left);
                    } else if (value is TimeSpan && format == "%d") {
                        worksheet.Cells[j, i].SetValue((int)((TimeSpan)value).TotalDays);
                    } else {
                        if (value is decimal) {
                            str = ((decimal)value).ToString(format);
                        } else if (value is DateTime) {
                            str = ((DateTime)value).ToString(format);
                        } else if (value is System.Windows.Media.Imaging.BitmapImage) {
                            continue;
                        } else {
                            str = value.ToString();
                        }

                        worksheet.Cells[j, i].SetValueAsText(str);
                        worksheet.Cells[j, i].SetHorizontalAlignment(RadHorizontalAlignment.Left);
                    }
                }
            }

            IWorkbookFormatProvider formatProvider = new XlsxFormatProvider();
            using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate)) {
                formatProvider.Export(workbook, output);
            }
        }
    }
}
