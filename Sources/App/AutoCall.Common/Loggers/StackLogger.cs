using AutoCall.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Common.Loggers {
    public static class StackLogger {

        private const int TOTAL = 10;

        private static object _lock = new object();

        private static Queue<Tuple<DateTime, string>> _logs = new Queue<Tuple<DateTime, string>>();

        public static void Push(string message) {
            lock (_lock) {
                _logs.Enqueue(new Tuple<DateTime, string>(DateTime.UtcNow, message));
                while(_logs.Count > TOTAL) {
                    _logs.Dequeue();
                }
            }
        }

        public static string PopAll() {
            var now = DateTime.UtcNow;
            return String.Join("\n", _logs.Reverse().Select(e => $"{StringExtension.ToString(now - e.Item1)}: {e.Item2}"));
        }
    }
}
