using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class KosmographViewModel
    {
        private KosmographModel model;
        private readonly Lazy<ObservableCollection<EditTagViewModel>> tags;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.tags = new Lazy<ObservableCollection<EditTagViewModel>>(() =>
            {
                var tags = new ObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t)));
                tags.CollectionChanged += this.Tags_CollectionChanged;
                return tags;
            });
        }

        public KosmographModel Model => this.model;

        public ObservableCollection<EditTagViewModel> Tags => this.tags.Value;

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.OfType<EditTagViewModel>().Any())
            {
                this.model.Tags.Upsert(e.NewItems.OfType<EditTagViewModel>().Single().Model);
            }
        }
    }
}