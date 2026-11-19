using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoDispatch.Logic.Helpers {
    public static class DelayHelper {
        private static Stopwatch stopwatch = null;
        // при 50 меньше всего лагает и быстрее работает RefreshWrappers 5сек. 100 -> 8сек
        private static readonly TimeSpan elapsed = TimeSpan.FromMilliseconds(50);
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(50);

        public static void Restart() {
            stopwatch = Stopwatch.StartNew();
        }

        public static async Task DelayAsync() {
            await Interval();
        }
        public static async Task Interval() {
            if (stopwatch.Elapsed > elapsed) {
                await Task.Delay(delay);
                stopwatch.Restart();
            }
        }
    }
}
