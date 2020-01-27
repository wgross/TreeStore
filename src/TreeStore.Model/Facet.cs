using TreeStore.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model
{
    public class Facet : NamedBase
    {
        #region Construction and initialization of this instance

        public Facet(string name, params FacetProperty[] properties)
            : base(name)
        {
            this.Properties = properties.ToList();
        }

        public Facet()
            : base(string.Empty)
        { }

        #endregion Construction and initialization of this instance

        public ICollection<FacetProperty> Properties { get; set; } = new List<FacetProperty>();

        public void AddProperty(FacetProperty property)
        {
            if (this.Properties.Any(p => p.Name.Equals(property.Name)))
                throw new InvalidOperationException($"duplicate property name: {property.Name}");

            this.Properties = this.Properties.Union(property.Yield()).ToList();
        }

        public void RemoveProperty(FacetProperty property)
        {
            this.Properties = this.Properties.Where(p => !p.Equals(property)).ToList();
        }
    }
}