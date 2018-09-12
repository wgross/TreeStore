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
        private Lazy<ObservableCollection<EditEntityViewModel>> entities;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)> changesAtTags;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditEntityViewModel>)> changesAtEntities;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
            this.changesAtTags = new List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)>();
            this.changesAtEntities = new List<(NotifyCollectionChangedAction, IEnumerable<EditEntityViewModel>)>();

            this.DeleteTagCommand = new RelayCommand<EditTagViewModel>(this.DeleteTagExecuted);
            this.DeleteEntityCommand = new RelayCommand<EditEntityViewModel>(this.DeleteEntityExecuted);

            this.Rollback();
        }

        public KosmographModel Model => this.model;

        public ObservableCollection<EditTagViewModel> Tags => this.tags.Value;

        public ObservableCollection<EditEntityViewModel> Entities => this.entities.Value;

        public EditTagViewModel SelectedTag { get; set; }

        #region Remove tag from model

        public ICommand DeleteTagCommand { get; }

        private void DeleteTagExecuted(EditTagViewModel tag) => this.Tags.Remove(tag);

        private void RemoveTag(Tag tag) => this.model.Tags.Delete(tag.Id);

        #endregion Remove tag from model

        #region Create new Tag in model

        //public ICommand CreateTagCommand { get; }

        //private void CreateTagExecuted(string name)
        //{
        //    this.SelectedTag = CreateNewTag();
        //}

        public EditTagViewModel CreateNewTag() => new EditTagViewModel(new Tag(string.Empty, new Facet()), this.OnTagCommitted);

        #endregion Create new Tag in model

        #region Create new Entity in Model

        public EditEntityViewModel CreateNewEntity() => new EditEntityViewModel(new Entity(string.Empty, new Tag(string.Empty, new Facet())), this.OnEntityCommitted);

        #endregion Create new Entity in Model

        #region Delete Entity from Model

        public ICommand DeleteEntityCommand { get; }

        private void DeleteEntityExecuted(EditEntityViewModel entity) => this.Entities.Remove(entity);

        private void RemoveEntity(Entity entity) => this.model.Entities.Delete(entity.Id);

        #endregion Delete Entity from Model

        #region Commit changes of Tags to model

        private void OnTagCommitted(Tag tag)
        {
            this.model.Tags.Upsert(tag);
        }

        private void OnEntityCommitted(Entity entity)
        {
            this.model.Entities.Upsert(entity);
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

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems.OfType<EditEntityViewModel>().Any())
                    {
                        this.changesAtEntities.Add((e.Action, e.NewItems.OfType<EditEntityViewModel>().ToArray()));
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.OfType<EditEntityViewModel>().Any())
                    {
                        this.changesAtEntities.Add((e.Action, e.OldItems.OfType<EditEntityViewModel>().ToArray()));
                    }
                    break;
            }
        }

        public void Commit()
        {
            this.CommitTags();
            this.CommitEntities();
        }

        private void CommitTags()
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

        public void CommitEntities()
        {
            foreach (var (action, entities) in this.changesAtEntities)
            {
                switch (action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var entity in entities)
                            this.OnEntityCommitted(entity.Model);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var entity in entities)
                            this.RemoveEntity(entity.Model);
                        break;
                }
            }
            this.changesAtEntities.Clear();
        }

        public void Rollback()
        {
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
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

        private void CreateLazyEntitiesCollection()
        {
            this.entities = new Lazy<ObservableCollection<EditEntityViewModel>>(() =>
            {
                var entites = new ObservableCollection<EditEntityViewModel>(this.model.Entities.FindAll().Select(e => new EditEntityViewModel(e, this.OnEntityCommitted)));
                entites.CollectionChanged += this.Entities_CollectionChanged;
                return entites;
            });
            this.RaisePropertyChanged(nameof(Entities));
        }

        #endregion Commit changes of Tags to model
    }
}