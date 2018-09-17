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
            if(r.From != null )
                this.From = new EntityViewModel(r.From);
            if(r.To != null)
                this.To = new EntityViewModel(r.To);
            this.Tags = new ObservableCollection<TagViewModel>(r.Tags.Select(t => new TagViewModel(t)));
        }

        public EntityViewModel From
        {
            get => this.from;
            set
            {
                this.from = value;
                this.Model.From = value?.Model;
                this.RaisePropertyChanged(nameof(From));
            }
        }

        private EntityViewModel from;

        public EntityViewModel To
        {
            get => this.to;
            set
            {
                this.to = value;
                this.Model.To = value?.Model;
                this.RaisePropertyChanged(nameof(To));
            }
        }

        private EntityViewModel to;

        public ObservableCollection<TagViewModel> Tags { get; }
    }
}