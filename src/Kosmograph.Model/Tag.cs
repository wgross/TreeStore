using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Tag : FacetedEntityBase
    {
        public Tag()
            : base(Facet.Empty)
        { }

        public Tag(Facet facet)
            : base(facet)
        { }
    }
}