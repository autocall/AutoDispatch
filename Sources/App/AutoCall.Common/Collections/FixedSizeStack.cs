using System.Collections.Generic;
using System.Linq;

namespace AutoCall.Common.Collections {
    public class FixedSizeStack<T> {
        private readonly List<T> list = new List<T>();
        public int Limit { get; set; }

        public FixedSizeStack(int limit) {
            Limit = limit;
        }

        public void Push(T obj) {
            list.Add(obj);
            while (list.Count > Limit) {
                list.RemoveAt(0);
            }
        }

        public T Pop() {
            var elem = list.Last();
            list.Remove(elem);
            return elem;
        }

        public int Count => list.Count;
    }
}
