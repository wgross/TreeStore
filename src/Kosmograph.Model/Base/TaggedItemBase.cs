using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model.Base
{
    public abstract class TaggedItemBase : NamedItemBase
    {
        public TaggedItemBase(string name, Tag[] tags)
            : base(name)
        {
            this.Tags = tags.ToList();
        }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public void AddTag(Tag tag)
        {
            this.Tags = this.Tags.Union(tag.Yield()).ToList();
        }

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        public void SetFacetProperty<T>(FacetProperty facetProperty, T value)
        {
            this.Values[facetProperty.Id.ToString()] = value;
        }

        public (bool, object) TryGetFacetProperty(FacetProperty facetProperty)
        {
            return (this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);
        }
    }
}