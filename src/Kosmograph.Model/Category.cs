using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Category : EntityBase
    {
        public Facet Facet { get; private set; }
        public IEnumerable<Category> SubCategories { get; private set; } = Enumerable.Empty<Category>();
        public Category Parent { get; set; }

        public void AssignFacet(Facet facet)
        {
            this.Facet = facet;
        }

        public void AddSubCategory(Category subcategory)
        {
            subcategory.Parent = this;
            this.SubCategories = this.SubCategories.Union(new[] { subcategory });
        }
    }
}