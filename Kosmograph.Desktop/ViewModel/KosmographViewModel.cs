using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class KosmographViewModel : ViewModelBase
    {
        private KosmographModel model;
        private Lazy<ObservableCollection<EditTagViewModel>> tags;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)> changesAtTags;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.CreateLazyTagsCollection();
            this.changesAtTags = new List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)>();

            this.DeleteTagCommand = new RelayCommand<EditTagViewModel>(this.DeleteTagExecuted);
            this.CreateTagCommand = new RelayCommand<string>(this.CreateTagExecuted);

            this.Rollback();
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

        public ICommand CreateTagCommand { get; }

        private void CreateTagExecuted(string name)
        {
            this.SelectedTag = CreateNewTag();
        }

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
                        this.changesAtTags.Add((e.Action, e.NewItems.OfType<EditTagViewModel>().ToArray()));
                        //this.OnTagCommitted(e.NewItems.OfType<EditTagViewModel>().Single().Model);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.OfType<EditTagViewModel>().Any())
                    {
                        this.changesAtTags.Add((e.Action, e.OldItems.OfType<EditTagViewModel>().ToArray()));
                        // this.RemoveTag(e.OldItems.OfType<EditTagViewModel>().Single().Model);
                    }
                    break;
            }
        }

        public void Commit()
        {
            foreach (var (action, tags) in this.changesAtTags)
            {
                switch (action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var tag in tags)
                            this.OnTagCommitted(tag.Model);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var tag in tags)
                            this.RemoveTag(tag.Model);
                        break;
                }
            }
            this.changesAtTags.Clear();
        }

        public void Rollback()
        {
            this.CreateLazyTagsCollection();
        }

        private void CreateLazyTagsCollection()
        {
            this.tags = new Lazy<ObservableCollection<EditTagViewModel>>(() =>
            {
                var tags = new ObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.OnTagCommitted)));
                tags.CollectionChanged += this.Tags_CollectionChanged;
                return tags;
            });
            this.RaisePropertyChanged(nameof(Tags));
        }

        #endregion Commit changes of Tags to model
    }
}