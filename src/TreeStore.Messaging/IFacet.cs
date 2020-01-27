using System.Collections.Generic;

namespace TreeStore.Messaging
{
    public interface IFacet<FacetPropertyImpl> : INamed
        where FacetPropertyImpl : IFacetProperty
    {
        IEnumerable<IFacetProperty> Properties { get; }
    }
}