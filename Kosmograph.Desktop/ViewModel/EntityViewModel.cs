using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class EntityViewModel : NamedViewModelBase<Entity>
    {
        public EntityViewModel(Entity model) : base(model)
        {
            this.Tags = new ObservableCollection<AssignedTagViewModel>(model.Tags.Select(t => new AssignedTagViewModel(t, model.Values)));
            this.Tags.CollectionChanged += this.Tags_CollectionChanged;
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var t in e.NewItems.OfType<AssignedTagViewModel>())
                        this.Model.Tags.Add(t.Model);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var t in e.OldItems.OfType<AssignedTagViewModel>())
                        this.Model.Tags.Remove(t.Model);
                    break;
            }
        }

        public ObservableCollection<AssignedTagViewModel> Tags { get; set; }
    }
}