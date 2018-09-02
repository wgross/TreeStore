using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T instance) where T : class
        {
            if(instance is null) yield break;
            yield return instance;
        }
    }
}