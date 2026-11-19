using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCall.Common.Collections {
    public static class CollectionExtension {
        /// <summary>
        ///     Замена ForEach из Microsoft.Practices.ObjectBuilder2 (Unity), которым пользовался код V1 </summary>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action) {
            foreach (T item in sequence) {
                action(item);
            }
        }


        public static IEnumerable<List<T>> DivideBy<T>(this List<T> queries, int limit = 1000) {
            if (queries.Count > 0) {
                if (queries.Count <= limit) {
                    yield return queries;
                } else {
                    for (int i = 0; i < queries.Count; i += limit) {
                        yield return queries.Skip(i).Take(limit).ToList();
                    }
                }
            }
        }
        public static void CreateOrAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value) {
            if (dictionary.ContainsKey(key) == false) {
                dictionary[key] = new List<TValue>();
            }
            dictionary[key].Add(value);
        }

        public static void CreateOrAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, IEnumerable<TValue> values) {
            if (dictionary.ContainsKey(key) == false) {
                dictionary[key] = new List<TValue>();
            }
            dictionary[key].AddRange(values);
        }

        public static void CreateOrAdd<TKey, TValue>(this BlockDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value) {
            if (dictionary.ContainsKey(key) == false) {
                dictionary[key] = new List<TValue>();
            }
            dictionary[key].Add(value);
        }

        public static void CreateOrAdd<TKey, TValue>(this BlockDictionary<TKey, List<TValue>> dictionary, TKey key, IEnumerable<TValue> values) {
            if (dictionary.ContainsKey(key) == false) {
                dictionary[key] = new List<TValue>();
            }
            dictionary[key].AddRange(values);
        }

        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> list) {
            foreach (var item in list) {
                collection.Add(item);
            }
        }
        public static void RemoveRange<T>(this IList<T> collection, IEnumerable<T> list) {
            foreach (var item in list) {
                collection.Remove(item);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source) {
                if (seenKeys.Add(keySelector(element))) {
                    yield return element;
                }
            }
        }

        public static void Move<T>(this List<T> list, T item, int newIndex) {
            var oldIndex = list.IndexOf(item);
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, item);
        }
    }
}
