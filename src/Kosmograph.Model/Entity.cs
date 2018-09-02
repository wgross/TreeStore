using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : EntityBase
    {
        #region Entity has Catagories

        public Category Category { get; private set; }

        public IEnumerable<Facet> Facets()
        {
            if (this.Category is null)
                return this.Tags.Select(t => t.Facet);
            return this.Category.Facets().Union(this.Tags.Select(t => t.Facet));
        }

        public void SetCategory(Category category)
        {
            this.Category = category;
        }

        #endregion Entity has Catagories

        #region Entity has Tags

        public IEnumerable<Tag> Tags { get; private set; } = Enumerable.Empty<Tag>();

        public void AddTag(Tag tag)
        {
            this.Tags = this.Tags.Union(tag.Yield());
        }

        #endregion Entity has Tags
    }
}