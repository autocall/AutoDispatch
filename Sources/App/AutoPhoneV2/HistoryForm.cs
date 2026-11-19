using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AutoPhoneV2 {
    public partial class HistoryForm : Form {

        public List<HistoryItem> _historyCalls = new List<HistoryItem>();
        public HistoryItem SelectedHistoryCall { get; set; }
        public HistoryItem SelectedHistoryDetals { get; set; }

        public HistoryForm() {
            InitializeComponent();

            this.TopMost = true;
        }

        public class HistoryItem {
            public string Number { get; set; }
            public string Header { get; set; }
            public DateTime DateTime { get; set; }
            public object Model { get; set; }
        }

        public void SetSource(List<Tuple<string, string, DateTime, object>> historyCalls) {
            //if (historyCalls.Any() == false) {
            //    historyCalls["(800) 123-45-61"] = new Tuple<string, DateTime>("Item1", DateTime.Now);
            //    historyCalls["(800) 123-45-62"] = new Tuple<string, DateTime>("Item1", DateTime.Now);
            //    historyCalls["(800) 123-45-63"] = new Tuple<string, DateTime>("Item1", DateTime.Now);
            //    historyCalls["(800) 123-45-64"] = new Tuple<string, DateTime>("Item1", DateTime.Now);
            //}

            //dataGridView1.ColumnCount = 3;
            //dataGridView1.Columns[0].Name = "Header";
            //dataGridView1.Columns[1].Name = "Time";
            //dataGridView1.Columns[2].Name = "Number";

            foreach (var call in historyCalls.OrderByDescending(e => e.Item3)) {
                dataGridView1.Rows.Add(new string[] { call.Item2, call.Item3.ToString("h:mm tt"), call.Item1 });
                _historyCalls.Add(new HistoryItem() { Number = call.Item1, Header = call.Item2, DateTime = call.Item3, Model = call.Item4 });
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e) {
            dataGridView1.ClearSelection();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.ColumnIndex == 2) {
                Close();
                if (e.RowIndex != -1 && e.RowIndex < _historyCalls.Count) {
                    SelectedHistoryCall = _historyCalls[e.RowIndex];
                }
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.ColumnIndex == 0 || e.ColumnIndex == 1) {
                if (e.RowIndex != -1 && e.RowIndex < _historyCalls.Count) {
                    if (_historyCalls[e.RowIndex] != null && _historyCalls[e.RowIndex].Model != null) {
                        SelectedHistoryDetals = _historyCalls[e.RowIndex];
                        Close();
                    } else {
                        MessageBox.Show("No Data");
                    }
                }
            }
        }
    }
}
