using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class EntityViewModel : NamedViewModelBase<Entity>
    {
        public EntityViewModel(Entity model) : base(model)
        {
            this.Tags = new ObservableCollection<TagViewModel>(model.Tags.Select(t => new TagViewModel(t)));
        }

        public ObservableCollection<TagViewModel> Tags { get; set; }
    }
}