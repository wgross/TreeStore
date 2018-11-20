using System;
using System.Collections.Generic;

namespace Kosmograph.Desktop
{
    public class IdentityMap<TSource,TTarget>
    {
        private readonly Dictionary<TSource, TTarget> source2target = new Dictionary<TSource, TTarget>();

        public void Add(TSource sourceId, TTarget targetId) => this.source2target.Add(sourceId, targetId);

        public bool TryGetTarget(TSource sourceId, out TTarget targetId) => this.source2target.TryGetValue(sourceId, out targetId);

        public bool Remove(TSource sourceId) => this.source2target.Remove(sourceId);
    }
}