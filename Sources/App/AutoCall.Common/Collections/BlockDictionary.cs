using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AutoCall.Common.Collections {
    public class BlockDictionary<TKey, TValue> {

        private Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();
        private object _lock = new object();
        //private bool _usingLock = false;

        public TValue this[TKey key] {
            get {
                lock (_lock) {
                    return Dictionary[key];
                }
            }
            set {
                lock (_lock) {
                    Dictionary[key] = value;
                }
            }
        }

        public int Count {
            get {
                lock (_lock) {
                    return Dictionary.Count;
                }
            }
        }

        public void Clear() {
            lock (_lock) {
                Dictionary.Clear();
            }
        }

        public bool ContainsKey(TKey key) {
            lock (_lock) { 
                return Dictionary.ContainsKey(key);
            }
        }

        public bool ContainsKey(TKey key, out int count) {
            lock (_lock) {
                count = Dictionary.Count;
                return Dictionary.ContainsKey(key);
            }
        }

        public void Remove(TKey key) {
            lock (_lock) {
                Dictionary.Remove(key);
            }
        }

        public List<KeyValuePair<TKey, TValue>> ToList1() {
            lock (_lock) {
                return Dictionary.ToList();
            }
        }

        public bool TryGetValue(TKey key, out TValue value) {
            lock (_lock) {
                return Dictionary.TryGetValue(key, out value);
            }
        }

        public List<TValue> Values {
            get {
                lock (_lock) {
                    return Dictionary.Values.ToList();
                }
            }
        }

        public bool Any() {
            lock (_lock) {
                return Dictionary.Count > 0;
            }
        }

        public bool Any(Func<KeyValuePair<TKey, TValue>, bool> predicate) {
            lock (_lock) {
                return Dictionary.Any(predicate);
            }
        }

        //public IDisposable Lock() {
        //    while (_usingLock == true) {
        //        Thread.Sleep(100);
        //    }
        //    _usingLock = true;
        //    return new BlockDictionaryLocker<TKey, TValue>(this);
        //}

        //internal void Release() {
        //    _usingLock = false;
        //}
    }

    //public class BlockDictionaryLocker<TKey, TValue> : IDisposable {
    //    private BlockDictionary<TKey, TValue> _dictionary;

    //    public BlockDictionaryLocker(BlockDictionary<TKey, TValue> dictionary) {
    //        this._dictionary = dictionary;
    //    }

    //    public void Dispose() {
    //        this._dictionary.Release();
    //        this._dictionary = null;
    //    }
    //}
}
