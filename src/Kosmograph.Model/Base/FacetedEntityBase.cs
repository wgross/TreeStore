namespace Kosmograph.Model.Base
{
    public class FacetedEntityBase : EntityBase
    {
        public FacetedEntityBase(Facet facet)
        {
            this.Facet = facet;
        }

        public FacetedEntityBase()
            : this(null)
        {
        }

        public Facet Facet { get; private set; }

        public void AssignFacet(Facet facet) => this.Facet = facet;
    }
}