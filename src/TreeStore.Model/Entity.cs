using TreeStore.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model
{
    public sealed class Entity : TaggedBase, Messaging.IEntity, ICloneable
    {
        #region Construction and initialization of this instance

        public Entity(string name, params Tag[] tags)
            : this(name, tags.AsEnumerable())
        {
        }

        public Entity(string name, IEnumerable<Tag> tags)
            : base(name, tags.ToArray())
        {
        }

        private Entity(string name, IEnumerable<Tag> tags, IDictionary<string, object?> values)
            : base(name, tags.ToArray())
        {
            this.Values = new Dictionary<string, object?>(values);
        }

        public Entity()
            : base(string.Empty, new Tag[0])
        { }

        #endregion Construction and initialization of this instance

        #region Entity has Categories

        public Category? Category { get; set; }

        public string UniqueName
        {
            get => $"{this.Name.ToLower()}_{this.Category!.Id}";
            set { }
        }

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

        public object Clone() => new Entity(this.Name, this.Tags.ToArray(), this.Values);

        #endregion Entity has Categories
    }
}