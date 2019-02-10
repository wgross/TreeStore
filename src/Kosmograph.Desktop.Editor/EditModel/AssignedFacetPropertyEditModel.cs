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
            get => this.value ?? this.Value;
            set => this.Set(nameof(Value), ref this.value, value);
        }

        private object value = null;

        override protected void Commit()
        {
            // this.Model.Value = this.Value;
        }

        override protected void Rollback()
        {
            this.value = null;
        }
    }
}