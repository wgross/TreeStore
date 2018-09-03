namespace Kosmograph.Model.Base
{
    public class FacetedEntityBase : EntityBase
    {
        public FacetedEntityBase(string name, Facet facet)
            : base(name)
        {
            this.Facet = facet;
        }

        public FacetedEntityBase()
            : base(string.Empty)
        {
        }

        public Facet Facet { get; set; } = Facet.Empty;

        public void AssignFacet(Facet facet) => this.Facet = facet;
    }
}