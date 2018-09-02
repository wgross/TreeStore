using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Entity : EntityBase
    {
        private readonly IDictionary<Guid, object> values = new Dictionary<Guid, object>();

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

        public IEnumerable<Tag> Tags { get; private set; } = Enumerable.Empty<Tag>();

        public void AddTag(Tag tag)
        {
            this.Tags = this.Tags.Union(tag.Yield());
        }

        #endregion Entity has Tags

        #region Entity has FacetProperty values

        public void SetFacetProperty<T>(FacetProperty facetProperty, T value)
        {
            this.values[facetProperty.Id] = value;
        }

        public (bool, object) TryGetFacetProperty(FacetProperty facetProperty)
        {
            return (this.values.TryGetValue(facetProperty.Id, out var value), value);
        }

        #endregion Entity has FacetProperty values
    }
}