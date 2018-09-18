using Kosmograph.Model;
using System.Collections.Generic;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedFacetPropertyViewModel : NamedViewModelBase<FacetProperty>
    {
        private readonly IDictionary<string, object> values;

        public AssignedFacetPropertyViewModel(FacetProperty model, IDictionary<string, object> values)
            : base(model)
        {
            this.values = values;
        }

        public object Value
        {
            get => this.values.TryGetValue(this.Model.Id.ToString(), out var value) ? value : null;
            set
            {
                this.values[this.Model.Id.ToString()] = value;
                this.RaisePropertyChanged(nameof(Value));
            }
        }
    }
}