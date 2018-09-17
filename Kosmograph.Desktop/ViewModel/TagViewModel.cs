using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class TagViewModel : NamedViewModelBase<Tag>
    {
        public TagViewModel(Tag model)
             : base(model)
        {
            this.Properties = new ObservableCollection<FacetPropertyViewModel>(
                model.Facet.Properties.Select(p => new FacetPropertyViewModel(p)));
        }

        public ObservableCollection<FacetPropertyViewModel> Properties { get; set; }
    }
}