using Kosmograph.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kosmograph.Desktop.Lists.ViewModel
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

        public override void RaisePropertyChanged<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T), bool broadcast = false)
        {
            if (nameof(this.Name).Equals(propertyName))
                this.Model.Facet.Name = this.Name;
            // continue raising the after the facet is synced
            base.RaisePropertyChanged(propertyName, oldValue, newValue, broadcast);
        }
    }
}