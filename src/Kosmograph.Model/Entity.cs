using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : TaggedBase
    {
        #region Construction and initialization of this instance

        public Entity(string name, params Tag[] tags)
            : base(name, tags)
        {
            this.Tags = tags.ToList();
        }

        public Entity()
            : base(string.Empty, new Tag[0])
        {
        }

        #endregion Construction and initialization of this instance

        #region Entity has Categories

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

        #endregion Entity has Categories
    }
}