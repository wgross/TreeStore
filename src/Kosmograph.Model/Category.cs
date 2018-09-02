using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Category : FacetedEntityBase
    {
        public Category()
        {
        }

        public Category(Facet ownFacet, params Category[] subcategories)
            : base(ownFacet)
        {
            this.SubCategories = subcategories.Select(c =>
            {
                c.Parent = this;
                return c;
            });
        }

        #region Category owns a facet

        public IEnumerable<Facet> Facets()
        {
            if (this.Parent is null)
                return this.Facet.Yield();
            return this.Facet.Yield().Union(this.Parent.Facets());
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