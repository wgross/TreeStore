using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Category : EntityBase
    {
        public Category()
        {
        }

        public Category(Facet ownFacet, params Category[] subcategories)
        {
            this.OwnFacet = ownFacet;
            this.SubCategories = subcategories.Select(c =>
            {
                c.Parent = this;
                return c;
            });
        }

        #region Category owns a facet

        public IEnumerable<Facet> Facets()
        {
            if(this.Parent is null)
                return this.OwnFacet.Yield();
            return this.OwnFacet.Yield().Union(this.Parent.Facets());
        }

        public Facet OwnFacet { get; private set; }

        public void AssignFacet(Facet facet)
        {
            this.OwnFacet = facet;
        }

        #endregion Category owns a facet

        #region Category is hierarchical

        public IEnumerable<Category> SubCategories { get; private set; } = Enumerable.Empty<Category>();

        public Category Parent { get; set; }

        public void AddSubCategory(Category subcategory)
        {
            subcategory.Parent = this;
            this.SubCategories = this.SubCategories.Union(new[] { subcategory });
        }

        #endregion Category is hierarchical
    }
}