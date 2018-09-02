using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : EntityBase
    {
        public Category Category { get; private set; }

        public IEnumerable<Facet> Facets() => this.Category.Facets();

        public void SetCategory(Category category)
        {
            this.Category = category;
        }
    }
}