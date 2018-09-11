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
                var tags = new ObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.CommitTag)));
                tags.CollectionChanged += this.Tags_CollectionChanged;
                return tags;
            });
        }

        public KosmographModel Model => this.model;

        public ObservableCollection<EditTagViewModel> Tags => this.tags.Value;

        public EditTagViewModel SelectedTag { get; set; }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.OfType<EditTagViewModel>().Any())
            {
                this.CommitTag(e.NewItems.OfType<EditTagViewModel>().Single().Model);
            }
        }

        private void CommitTag(Tag tag)
        {
            this.model.Tags.Upsert(tag);
        }

        public EditTagViewModel CreateNewTag() => new EditTagViewModel(new Tag(string.Empty, new Facet()), this.CommitTag);
    }
}