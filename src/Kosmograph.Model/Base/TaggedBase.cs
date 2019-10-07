using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model.Base
{
    public abstract class TaggedBase : NamedBase
    {
        public TaggedBase(string name, Tag[] tags)
            : base(name)
        {
            this.Tags = tags.ToList();
        }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public void AddTag(Tag tag)
        {
            this.Tags = this.Tags.Union(tag.Yield()).ToList();
        }

        public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object>();

        public void SetFacetProperty<T>(FacetProperty facetProperty, T value)
            => this.Values[facetProperty.Id.ToString()] = value;

        public (bool exists, object? value) TryGetFacetProperty(FacetProperty facetProperty)
            => (this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);
    }
}