using System.Collections.Generic;

namespace Kosmograph.Structure
{
    public interface ITagged<TagImpl, FacetImpl, FacetPropertyImpl> where TagImpl : ITag<FacetImpl, FacetPropertyImpl>
        where FacetImpl : IFacet<FacetPropertyImpl>
        where FacetPropertyImpl : IFacetProperty
    {
        IEnumerable<TagImpl> Tags { get; }
    }
}