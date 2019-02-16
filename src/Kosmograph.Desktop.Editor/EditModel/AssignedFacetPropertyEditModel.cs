using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Collections.Generic;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class AssignedFacetPropertyEditModel : EditModelBase
    {
        public AssignedFacetPropertyEditModel(FacetProperty model, IDictionary<string, object> values)
        {
            this.Model = model;
            this.Values = values;
        }

        public FacetProperty Model { get; }
        public IDictionary<string, object> Values { get; }

        public object Value
        {
            get => this.value ?? this.ResolvePropertyValue();
            set => this.Set(nameof(Value), ref this.value, value);
        }

        private object ResolvePropertyValue()
        {
            if (this.Values.TryGetValue(this.Model.Id.ToString(), out var value))
                return value;
            return null;
        }

        private object value = null;

        override protected void Commit()
        {
            if (this.value is null)
                return; // value of null isnt committable!

            this.Values[this.Model.Id.ToString()] = this.value;
        }

        override protected void Rollback()
        {
            this.value = null;
        }
    }
}