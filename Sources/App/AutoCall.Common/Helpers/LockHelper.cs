using System;
using System.Threading.Tasks;

namespace AutoCall.Common.Helpers {
    public static class LockHelper {

        public static bool Locked = false;

        public static async Task<IDisposable> EnterAsync() {
            while (Locked) {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
            Locked = true;
            return new ParallelHelper2Disposable();
        }

        public static void Release() {
            Locked = false;
        }

        public static void Reset() {
            Locked = false;
        }
    }

    public class ParallelHelper2Disposable : IDisposable {
        public void Dispose() {
            LockHelper.Release();
        }
    }
}
