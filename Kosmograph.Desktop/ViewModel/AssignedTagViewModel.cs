using Kosmograph.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedTagViewModel : NamedViewModelBase<Tag>
    {
        private Tag t;

        public AssignedTagViewModel(Tag model, IDictionary<string, object> values)
            : base(model)
        {
            this.t = model;
            this.Properties = new ObservableCollection<AssignedFacetPropertyViewModel>(model.Facet.Properties.Select(p => new AssignedFacetPropertyViewModel(p, values)));
        }

        public ObservableCollection<AssignedFacetPropertyViewModel> Properties { get; }
    }
}