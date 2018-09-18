using System.Collections.Generic;
using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class FacetPropertyViewModel : NamedViewModelBase<FacetProperty>
    {
        public FacetPropertyViewModel(FacetProperty model)
            : base(model)
        {
        }

        public IEnumerable<object> Value { get; set; }
    }
}