using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : EntityBase
    {
        #region Construction and initialization of this instance

        public Entity(string name, params Tag[] tags)
            : base(name)
        {
            this.Tags = tags.ToList();
        }

        public Entity()
            : base(string.Empty)
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

        #region Entity has Tags

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public void AddTag(Tag tag)
        {
            this.Tags = this.Tags.Union(tag.Yield()).ToList();
        }

        #endregion Entity has Tags

        #region Entity has FacetProperty values

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        public void SetFacetProperty<T>(FacetProperty facetProperty, T value)
        {
            this.Values[facetProperty.Id.ToString()] = value;
        }

        public (bool, object) TryGetFacetProperty(FacetProperty facetProperty)
        {
            return (this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);
        }

        #endregion Entity has FacetProperty values
    }
}