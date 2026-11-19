using System;
using System.Threading;

namespace AutoCall.Common.Helpers {
    public static class ParallelHelper {

        private static int _lockCounter = 0;

        public static IDisposable Enter(int maxThreads = 1) {
            while (_lockCounter >= maxThreads) {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
            _lockCounter++;
            return new ParallelHelperDisposable();
        }

        public static void Release() {
            _lockCounter--;
        }

        public static void Reset() {
            _lockCounter = 0;
        }
    }

    public class ParallelHelperDisposable : IDisposable {
        public void Dispose() {
            ParallelHelper.Release();
        }
    }
}
