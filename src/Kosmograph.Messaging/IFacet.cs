using System.Collections.Generic;

namespace Kosmograph.Messaging
{
    public interface IFacet<FacetPropertyImpl> : INamed
        where FacetPropertyImpl : IFacetProperty
    {
        IEnumerable<IFacetProperty> Properties { get; }
    }
}