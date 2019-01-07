using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class RelationshipViewModel : NamedViewModelBase<Relationship>
    {
        public RelationshipViewModel(Relationship model, EntityViewModel from, EntityViewModel to, params TagViewModel[] tags)
            : base(model)
        {
            this.From = from;
            this.To = to;
            this.Tags = new ObservableCollection<AssignedTagViewModel>(tags.Select(t => new AssignedTagViewModel(t, model.Values)));
            this.Tags.CollectionChanged += this.Tags_CollectionChanged;
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var t in e.NewItems.OfType<AssignedTagViewModel>())
                        this.Model.Tags.Add(t.Tag.Model);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var t in e.OldItems.OfType<AssignedTagViewModel>())
                        this.Model.Tags.Remove(t.Tag.Model);
                    break;
            }
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

        public ObservableCollection<AssignedTagViewModel> Tags { get; }
    }
}