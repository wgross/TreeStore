using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class EntityViewModel : NamedViewModelBase<Entity>
    {
        public EntityViewModel(Entity model, params TagViewModel[] tags) : base(model)
        {
            this.Tags = new ObservableCollection<AssignedTagViewModel>(this.Model.Tags.Select(t => new AssignedTagViewModel(new TagViewModel(t), model.Values)));
            // this.Tags.CollectionChanged += this.Tags_CollectionChanged;
        }

        //private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            foreach (var t in e.NewItems.OfType<AssignedTagViewModel>())
        //                this.Model.Tags.Add(t.Tag.Model);
        //            break;

        //        case NotifyCollectionChangedAction.Remove:
        //            foreach (var t in e.OldItems.OfType<AssignedTagViewModel>())
        //                this.Model.Tags.Remove(t.Tag.Model);
        //            break;
        //    }
        //}

        public ObservableCollection<AssignedTagViewModel> Tags { get; }
    }
}