namespace Kosmograph.Structure
{
    public interface IEntity<TagImpl, FacetImpl, FacetPropertyImpl> : ITagged<TagImpl, FacetImpl, FacetPropertyImpl>, INamed
        where TagImpl : ITag<FacetImpl, FacetPropertyImpl>
        where FacetImpl : IFacet<FacetPropertyImpl>
        where FacetPropertyImpl : IFacetProperty
    {
    }
}