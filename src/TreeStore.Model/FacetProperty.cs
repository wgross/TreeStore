using System;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public class FacetProperty : NamedBase
    {
        public FacetPropertyTypeValues Type { get; set; }

        #region Construction and initialization of this instance

        public FacetProperty(string name)
            : this(name, FacetPropertyTypeValues.String)
        { }

        public FacetProperty()
            : base()
        {
        }

        public FacetProperty(string name, FacetPropertyTypeValues type)
            : base(name)

        {
            this.Type = type;
        }

        #endregion Construction and initialization of this instance

        public bool CanAssignValue(string? value)
        {
            if (value is null)
                return true; // facet property is nullable

            switch (this.Type)
            {
                case FacetPropertyTypeValues.Long:
                    return long.TryParse(value, out var _);

                case FacetPropertyTypeValues.Double:
                    return double.TryParse(value, out var _);

                case FacetPropertyTypeValues.Decimal:
                    return decimal.TryParse(value, out var _);

                case FacetPropertyTypeValues.Guid:
                    return Guid.TryParse(value, out var _);

                case FacetPropertyTypeValues.DateTime:
                    return DateTime.TryParse(value, out var _);

                case FacetPropertyTypeValues.Bool:
                    return bool.TryParse(value, out var _);
                // string values are always possible
                case FacetPropertyTypeValues.String:
                    return true;

                default:
                    return false;
            }
        }

        public bool CanAssignValue(object? value)
        {
            if (value is null)
                return true; // facet property is nullable

            switch (this.Type)
            {
                case FacetPropertyTypeValues.Long:
                    return value switch
                    {
                        // convertable
                        int _ => true,
                        short _ => true,
                        // assignable
                        _ => typeof(long).IsAssignableFrom(value.GetType())
                    };

                case FacetPropertyTypeValues.Double:
                    return value switch
                    {
                        // convertable
                        float _ => true,
                        // assignable
                        _ => typeof(double).IsAssignableFrom(value.GetType())
                    };

                case FacetPropertyTypeValues.Decimal:
                    return value switch
                    {
                        // convertable
                        float _ => true,
                        double _ => true,
                        short _ => true,
                        int _ => true,
                        long _ => true,
                        // assignable
                        _ => typeof(decimal).IsAssignableFrom(value.GetType())
                    };

                case FacetPropertyTypeValues.Guid:
                    return typeof(Guid).IsAssignableFrom(value.GetType());

                case FacetPropertyTypeValues.DateTime:
                    return typeof(DateTime).IsAssignableFrom(value.GetType());

                case FacetPropertyTypeValues.Bool:
                    return typeof(bool).IsAssignableFrom(value.GetType());

                // string values are always possible
                case FacetPropertyTypeValues.String:
                    return typeof(string).IsAssignableFrom(value.GetType());

                default:
                    return false;
            }
        }
    }
}