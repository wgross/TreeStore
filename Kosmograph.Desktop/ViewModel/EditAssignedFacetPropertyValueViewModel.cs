using Kosmograph.Model;
using System;
using System.Collections.Generic;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditAssignedFacetPropertyValueViewModel : NamedViewModelBase<FacetProperty>
    {
        private readonly IDictionary<string, object> values;

        public EditAssignedFacetPropertyValueViewModel(FacetProperty property, IDictionary<string, object> propertyValues)
            : base(property)
        {
            this.values = propertyValues;
        }

        public object Value
        {
            get => this.value ?? (this.values.TryGetValue(this.Model.Id.ToString(), out var value) ? value : null);
            set => this.Set(nameof(Value), ref this.value, value);
        }

        private object value = null;

        public void Commit()
        {
            this.values[this.Model.Id.ToString()] = this.Value;
        }

        public void Rollback()
        {
            this.value = null;
        }
    }
}