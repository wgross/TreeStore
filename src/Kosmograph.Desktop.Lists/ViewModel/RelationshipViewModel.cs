using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class RelationshipViewModel : NamedViewModelBase<Relationship>
    {
        public RelationshipViewModel(Relationship model)
            : base(model)
        {
            this.From = new EntityViewModel(this.Model.From);
            this.To = new EntityViewModel(this.Model.To);
            this.Tags = new ObservableCollection<AssignedTagViewModel>(this.Model.Tags.Select(t => new AssignedTagViewModel(new TagViewModel(t), model.Values)));
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

        public EntityViewModel From { get; }

        public EntityViewModel To { get; }

        public ObservableCollection<AssignedTagViewModel> Tags { get; }
    }
}