using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCall.Common.Helpers {
    public class MonitorHelper {
        public bool Enabled = true;

        private List<KeyValuePair<string, TimeSpan>> Times = new List<KeyValuePair<string, TimeSpan>>();

        private Dictionary<string, DateTime> Begins = new Dictionary<string, DateTime>();

        public void Begin(string key = "") {
            if (Enabled) {
                Begins[key] = DateTime.UtcNow;
            }
        }

        public void End(string key = "") {
            if (Enabled) {
                if (Begins.ContainsKey(key)) {
                    Times.Add(new KeyValuePair<string, TimeSpan>(key, DateTime.UtcNow - Begins[key]));
                }
            }
        }

        public IDisposable Monitor(string key = "") {
            if (Enabled) {
                Begin(key);
            }
            return new MonitorItem(key, this);
        }

        public T Monitor<T>(string key, Func<T> func) {
            if (Enabled) {
                Begin(key);
            }
            var result = func();
            if (Enabled) {
                End(key);
            }
            return result;
        }

        public T Monitor<T>(Func<T> func) {
            var key = "";
            if (Enabled) {
                Begin(key);
            }
            var result = func();
            if (Enabled) {
                End(key);
            }
            return result;
        }

        public async Task<T> Monitor<T>(string key, Func<Task<T>> func) {
            if (Enabled) {
                Begin(key);
            }
            var result = await func();
            if (Enabled) {
                End(key);
            }
            return result;
        }

        public string Print() {
            var result = String.Empty;
            if (Enabled) {
                End();
                var count = Times.Where(e => e.Key == "").Count();
                var total = Times.Where(e => e.Key == "").Sum(e => e.Value.Ticks);
                result += $"Monitoring Duration: {new TimeSpan(total).TotalSeconds:F1}sec / {count}calls\n";

                foreach (IGrouping<string, KeyValuePair<string, TimeSpan>> group in Times
                    .GroupBy(e => e.Key).OrderByDescending(e => e.Sum(e2 => e2.Value.Ticks))) {
                    if (group.Key == "") continue;

                    count = group.Count();
                    var groupTotal = group.Sum(e => e.Value.Ticks);
                    if (total != 0) {
                        result += $"{group.Key}: {groupTotal * 100 / total:F0}%, {new TimeSpan(groupTotal).TotalSeconds:F1}s\n";
                    }
                }

                Times.Clear();
                Begins.Clear();
            }
            return result;
        }
    }

    internal class MonitorItem : IDisposable {
        private string _key;
        private MonitorHelper _monitor;

        public MonitorItem(string key, MonitorHelper monitor) {
            this._key = key;
            this._monitor = monitor;
        }

        public void Dispose() {
            _monitor.End(this._key);
        }
    }
}
