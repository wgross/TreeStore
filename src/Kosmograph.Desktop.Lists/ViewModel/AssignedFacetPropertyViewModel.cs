using GalaSoft.MvvmLight;
using Kosmograph.Model;
using System.Collections.Generic;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class AssignedFacetPropertyViewModel : ViewModelBase
    {
        private readonly IDictionary<string, object> values;

        public AssignedFacetPropertyViewModel(FacetProperty viewModel, IDictionary<string, object> values)
        {
            this.Property = viewModel;
            this.values = values;
        }

        public FacetProperty Property { get; }

        public object Value
        {
            get => this.values.TryGetValue(this.Property.Id.ToString(), out var value) ? value : null;
            set
            {
                this.values[this.Property.Id.ToString()] = value;
                this.RaisePropertyChanged(nameof(Value));
            }
        }
    }
}