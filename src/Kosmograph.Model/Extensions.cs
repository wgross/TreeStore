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
    }
}