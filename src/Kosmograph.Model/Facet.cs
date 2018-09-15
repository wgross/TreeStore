using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Facet : NamedItemBase
    {
        #region Construction and initialization of this instance

        public static readonly Facet Empty = new Facet { Id = Guid.Empty };

        public Facet(string name, params FacetProperty[] properties)
            : base(name)
        {
            this.Properties = properties.ToList();
        }

        public Facet()
            : base(string.Empty)
        { }

        #endregion Construction and initialization of this instance

        public List<FacetProperty> Properties { get; set; } = new List<FacetProperty>();

        public void AddProperty(FacetProperty property)
        {
            this.Properties = this.Properties.Union(property.Yield()).ToList();
        }

        public void RemoveProperty(FacetProperty property)
        {
            this.Properties = this.Properties.Where(p => !p.Equals(property)).ToList();
        }
    }
}