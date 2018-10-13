using System.Collections.Generic;

namespace Kosmograph.Structure
{
    public interface IFacet<FacetPropertyImpl> : INamed
        where FacetPropertyImpl : IFacetProperty
    {
        IEnumerable<IFacetProperty> Properties { get; }
    }
}