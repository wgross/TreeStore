using GalaSoft.MvvmLight;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class AssignedTagViewModel : ViewModelBase
    {
        public AssignedTagViewModel(TagViewModel tagViewModel, IDictionary<string, object> values)
        {
            this.Tag = tagViewModel;
            this.Properties = new ObservableCollection<AssignedFacetPropertyViewModel>(tagViewModel.Properties.Select(p => new AssignedFacetPropertyViewModel(p, values)));
        }

        public TagViewModel Tag { get; }

        public ObservableCollection<AssignedFacetPropertyViewModel> Properties { get; }
    }
}