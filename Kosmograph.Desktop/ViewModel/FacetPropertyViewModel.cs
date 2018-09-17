using System.Collections.Generic;
using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class FacetPropertyViewModel : NamedViewModelBase<FacetProperty>
    {
        public FacetPropertyViewModel(FacetProperty model)
            : base(model)
        {
        }

        public object Value { get; set; }
    }
}