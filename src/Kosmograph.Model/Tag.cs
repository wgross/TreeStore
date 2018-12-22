using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Tag : FacetingEntityBase, Messaging.ITag
    {
        public Tag()
            : base(string.Empty, new Facet(string.Empty))
        { }

        public Tag(string name)
            : base(name, new Facet(name))
        { }

        public Tag(string name, Facet facet)
            : base(name, facet)
        { }
    }
}