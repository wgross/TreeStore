using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

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
                var tags = new ObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.OnTagCommitted)));
                tags.CollectionChanged += this.Tags_CollectionChanged;
                return tags;
            });

            this.DeleteTagCommand = new RelayCommand<EditTagViewModel>(this.DeleteTagExecuted);
        }

        public KosmographModel Model => this.model;

        public ObservableCollection<EditTagViewModel> Tags => this.tags.Value;

        public EditTagViewModel SelectedTag { get; set; }

        #region Remove tag from model

        public ICommand DeleteTagCommand { get; }

        private void DeleteTagExecuted(EditTagViewModel obj)
        {
            this.Tags.Remove(obj);
        }

        private void RemoveTag(Tag tag)
        {
            this.model.Tags.Delete(tag.Id);
        }

        #endregion Remove tag from model

        #region Create new Tag in model

        public EditTagViewModel CreateNewTag() => new EditTagViewModel(new Tag(string.Empty, new Facet()), this.OnTagCommitted);

        #endregion Create new Tag in model

        #region Commit changes of Tags to model

        private void OnTagCommitted(Tag tag)
        {
            this.model.Tags.Upsert(tag);
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems.OfType<EditTagViewModel>().Any())
                    {
                        this.OnTagCommitted(e.NewItems.OfType<EditTagViewModel>().Single().Model);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.OfType<EditTagViewModel>().Any())
                    {
                        this.RemoveTag(e.OldItems.OfType<EditTagViewModel>().Single().Model);
                    }
                    break;
            }
        }

        #endregion Commit changes of Tags to model
    }
}