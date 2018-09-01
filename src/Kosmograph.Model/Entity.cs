using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : EntityBase
    {
        public Category Category { get; private set; }

        public IEnumerable<Facet> Facets => new[] { this.Category?.Facet }.Where(f => f != null);

        public void SetCategory(Category category)
        {
            this.Category = category;
        }
    }
}