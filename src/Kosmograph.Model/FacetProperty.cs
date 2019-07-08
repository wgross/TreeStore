using Kosmograph.Model.Base;
using System;

namespace Kosmograph.Model
{
    public class FacetProperty : NamedBase
    {
        private string value;

        public FacetPropertyTypeValues Type { get; set; }

        #region Construction and initialization of this instance

        public FacetProperty(string name)
            : base(name)
        { }

        public FacetProperty()
            : base()
        {
        }

        public FacetProperty(string v, FacetPropertyTypeValues type)
        {
            this.value = v;
            this.Type = type;
        }

        public bool CanAssignValue(string v)
        {
            switch (this.Type)
            {
                case FacetPropertyTypeValues.Integer:
                    return long.TryParse(v, out var _);
                case FacetPropertyTypeValues.Double:
                    return double.TryParse(v, out var _);
                case FacetPropertyTypeValues.Decimal:
                    return decimal.TryParse(v, out var _);
                case FacetPropertyTypeValues.Guid:
                    return Guid.TryParse(v, out var _);
                case FacetPropertyTypeValues.DateTime:
                    return DateTime.TryParse(v, out var _);
                // string values are always possible
                case FacetPropertyTypeValues.String:
                    return true;
                default:
                    return false;
            }
        }

        #endregion Construction and initialization of this instance
    }
}