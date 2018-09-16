using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class RelationshipViewModel : NamedViewModelBase<Relationship>
    {
        public RelationshipViewModel(Relationship r)
            : base(r)
        {
            this.From = new EntityViewModel(r.From);
            this.To = new EntityViewModel(r.To);
            this.Tags = new ObservableCollection<TagViewModel>(r.Tags.Select(t => new TagViewModel(t)));
        }

        public EntityViewModel From { get; }

        public EntityViewModel To { get; }

        public ObservableCollection<TagViewModel> Tags { get; }
    }
}