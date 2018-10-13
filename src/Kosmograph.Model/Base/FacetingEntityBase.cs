namespace Kosmograph.Model.Base
{
    public class FacetingEntityBase : NamedBase
    {
        public FacetingEntityBase(string name, Facet facet)
            : base(name)
        {
            this.Facet = facet;
        }

        public FacetingEntityBase()
            : base(string.Empty)
        {
        }

        public Facet Facet { get; set; }

        public void AssignFacet(Facet facet) => this.Facet = facet;
    }
}