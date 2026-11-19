using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCall.Common.Collections {
    public class BlockList<T> {
        private List<T> _list = new List<T>();
        private object _lock = new object();

        public void Add(T item) {
            lock (_lock) {
                _list.Add(item);
            }
        }
        public void Remove(T item) {
            lock (_lock) {
                _list.Remove(item);
            }
        }
        public bool Contains(T item) {
            lock (_lock) {
                return _list.Contains(item);
            }
        }
        public void Clear() {
            lock (_lock) {
                _list.Clear();
            }
        }
        public List<T> ToList() {
            lock (_lock) {
                return _list.ToList();
            }
        }
        public int Count {
            get {
                lock (_lock) {
                    return _list.Count;
                }
            }
        }
        public T this[int index] {
            get {
                lock (_lock) {
                    return _list[index];
                }
            }
        }
        public bool Any() {
            lock (_lock) {
                return _list.Any();
            }
        }
        public bool Any(Func<T, bool> predicate) {
            lock (_lock) {
                return _list.Any(predicate);
            }
        }
    }
}