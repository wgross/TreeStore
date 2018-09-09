using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Tag : FacetingEntityBase
    {
        public Tag()
            : base(string.Empty, Facet.Empty)
        { }

        public Tag(string name, Facet facet)
            : base(name, facet)
        { }
    }
}