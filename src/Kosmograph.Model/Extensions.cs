using System;
using System.Collections.Generic;

namespace Kosmograph.Model
{
    public static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T instance) where T : class
        {
            if (instance is null) yield break;
            yield return instance;
        }

        public static Func<T> AsFunc<T>(this T instance) => () => instance;

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