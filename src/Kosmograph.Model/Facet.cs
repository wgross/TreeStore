using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Facet : EntityBase
    {
        public static readonly Facet Empty = new Facet { Id = Guid.Empty };

        public Facet(params FacetProperty[] properties)
        {
            this.Properties = properties.ToArray();
        }

        public IEnumerable<FacetProperty> Properties { get; set; } = Enumerable.Empty<FacetProperty>();

        public void AddProperty(FacetProperty property)
        {
            this.Properties = this.Properties.Union(property.Yield());
        }

        public void RemoveProperty(FacetProperty property)
        {
            this.Properties = this.Properties.Where(p => !p.Equals(property)).ToArray();
        }
    }
}