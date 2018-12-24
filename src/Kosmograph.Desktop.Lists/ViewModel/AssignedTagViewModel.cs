using GalaSoft.MvvmLight;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class AssignedTagViewModel : ViewModelBase
    {
        public AssignedTagViewModel(Tag tag, IDictionary<string, object> values)
        {
            this.Tag = tag;
            this.Properties = new ObservableCollection<AssignedFacetPropertyViewModel>(tag.Facet.Properties.Select(p => new AssignedFacetPropertyViewModel(p, values)));
        }

        public Tag Tag { get; }

        public ObservableCollection<AssignedFacetPropertyViewModel> Properties { get; }
    }
}