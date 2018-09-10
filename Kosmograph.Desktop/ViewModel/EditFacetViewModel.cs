using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditFacetPropertyViewModel : EditNamedViewModel<FacetProperty>
    {
        public EditFacetPropertyViewModel(FacetProperty property)
            : base(property)
        {
        }
    }

    public class EditFacetViewModel : EditNamedViewModel<Facet>
    {
        private readonly Facet facet;

        public EditFacetViewModel(Facet facet)
            : base(facet)
        {
            this.Properties = new ObservableCollection<EditFacetPropertyViewModel>(
                this.Edited.Properties.Select(p => new EditFacetPropertyViewModel(p)));
        }

        public ObservableCollection<EditFacetPropertyViewModel> Properties { get; }
    }
}