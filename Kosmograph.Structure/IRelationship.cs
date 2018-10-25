namespace Kosmograph.Structure
{
    public interface IRelationship<EntityImpl, TagImpl, FacetImpl, FacetPropertyImpl> : ITagged<TagImpl, FacetImpl, FacetPropertyImpl>
        where EntityImpl : IEntity<TagImpl, FacetImpl, FacetPropertyImpl>
        where TagImpl : ITag<FacetImpl, FacetPropertyImpl>
        where FacetImpl : IFacet<FacetPropertyImpl>
        where FacetPropertyImpl : IFacetProperty
    {
        EntityImpl From { get; }
        EntityImpl To { get; }
    }
}