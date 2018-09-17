using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            this.Properties.CollectionChanged += this.Properties_CollectionChanged;
        }

        public ObservableCollection<FacetPropertyViewModel> Properties { get; }

        private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in e.NewItems.OfType<FacetPropertyViewModel>())
                        this.Model.Facet.Properties.Add(p.Model);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in e.OldItems.OfType<FacetPropertyViewModel>())
                        this.Model.Facet.Properties.Remove(p.Model);
                    break;
            }
        }

        public Facet Facet { get; internal set; }
    }
}