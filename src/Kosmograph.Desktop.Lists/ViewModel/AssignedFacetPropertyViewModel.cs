using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class AssignedFacetPropertyViewModel : ViewModelBase
    {
        private readonly IDictionary<string, object> values;

        public AssignedFacetPropertyViewModel(FacetPropertyViewModel viewModel, IDictionary<string, object> values)
        {
            this.Property = viewModel;
            this.values = values;
        }

        public FacetPropertyViewModel Property { get; }

        public object Value
        {
            get => this.values.TryGetValue(this.Property.Model.Id.ToString(), out var value) ? value : null;
            set
            {
                this.values[this.Property.Model.Id.ToString()] = value;
                this.RaisePropertyChanged(nameof(Value));
            }
        }
    }
}