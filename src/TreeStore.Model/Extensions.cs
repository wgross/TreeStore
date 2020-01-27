using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T instance) where T : class
        {
            if (instance is null) yield break;
            yield return instance;
        }

        public static Func<T> AsFunc<T>(this T instance) => () => instance;

        private static readonly object Null = new object();

        public static Func<T>? AsFunc<T>(this Func<T>? instance)
        {
            object cachedResult = Null;
            return () =>
            {
                if (cachedResult == Null)
                    cachedResult = instance!.Invoke();
                return (T)cachedResult;
            };
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
            return items;
        }

        public static T IfExistsThen<T>(this T item, Action<T> thenDo) where T : class
        {
            if (item is null)
                return null;

            thenDo(item);
            return item;
        }
    }
}