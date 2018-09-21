using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Tag : FacetingEntityBase
    {
        public Tag()
            : base(string.Empty, new Facet())
        { }

        public Tag(string name)
            : base(name, new Facet())
        { }

        public Tag(string name, Facet facet)
            : base(name, facet)
        { }
    }
}