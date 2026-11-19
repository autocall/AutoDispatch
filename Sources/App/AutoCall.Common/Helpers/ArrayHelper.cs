using System;

namespace AutoCall.Common.Helpers {
    public static class ArrayHelper {
        public static T[] ArrayCombine<T>(T[] array1, T[] array2) {
            T[] newArray = new T[array1.Length + array2.Length];
            Array.Copy(array1, newArray, array1.Length);
            Array.Copy(array2, 0, newArray, array1.Length, array2.Length);
            return newArray;
        }
    }
}
