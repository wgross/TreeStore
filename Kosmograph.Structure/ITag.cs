namespace Kosmograph.Structure
{
    public interface ITag<FacetImpl, FacetPropertyImpl> : INamed
        where FacetImpl : IFacet<FacetPropertyImpl>
        where FacetPropertyImpl : IFacetProperty
    {
        FacetImpl Facet { get; }
    }
}