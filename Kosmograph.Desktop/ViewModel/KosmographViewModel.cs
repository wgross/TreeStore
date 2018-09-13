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
        private Lazy<CommitableObservableCollection<EditTagViewModel>> tags;
        private Lazy<CommitableObservableCollection<EditEntityViewModel>> entities;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)> changesAtTags;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditEntityViewModel>)> changesAtEntities;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
            this.changesAtTags = new List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)>();
            this.changesAtEntities = new List<(NotifyCollectionChangedAction, IEnumerable<EditEntityViewModel>)>();

            this.CreateTagCommand = new RelayCommand(this.CreateTagExecuted);
            this.DeleteTagCommand = new RelayCommand<EditTagViewModel>(this.DeleteTagExecuted);
            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.DeleteEntityCommand = new RelayCommand<EditEntityViewModel>(this.DeleteEntityExecuted);

            this.Rollback();
        }

        public KosmographModel Model => this.model;

        public ObservableCollection<EditTagViewModel> Tags => this.tags.Value;

        public ObservableCollection<EditEntityViewModel> Entities => this.entities.Value;

        public EditTagViewModel SelectedTag
        {
            get => this.selectedTag;
            set => this.Set(nameof(SelectedTag), ref this.selectedTag, value);
        }

        private EditTagViewModel selectedTag;

        public EditEntityViewModel SelectedEntity
        {
            get => this.selectedEntity;
            set => this.Set(nameof(SelectedEntity), ref this.selectedEntity, value);
        }

        private EditEntityViewModel selectedEntity;

        #region Remove tag from model

        public ICommand DeleteTagCommand { get; }

        private void DeleteTagExecuted(EditTagViewModel tag) => this.Tags.Remove(tag);

        private void RemoveTag(Tag tag) => this.model.Tags.Delete(tag.Id);

        #endregion Remove tag from model

        #region Create new Tag in model

        public ICommand CreateTagCommand { get; }

        private void CreateTagExecuted()
        {
            var tmp = new EditTagViewModel(new Tag("new tag", new Facet()), this.OnTagCommitted);
            this.Tags.Add(tmp);
            this.SelectedTag = tmp;
        }

        #endregion Create new Tag in model

        #region Create new Entity in Model

        public ICommand CreateEntityCommand { get; }

        public void CreateEntityExecuted()
        {
            var tmp = new EditEntityViewModel(new Entity("new entity", new Tag(string.Empty, new Facet())), this.OnEntityCommitted);
            this.Entities.Add(tmp);
            this.SelectedEntity = tmp;
        }

        #endregion Create new Entity in Model

        #region Delete Entity from Model

        public ICommand DeleteEntityCommand { get; }

        private void DeleteEntityExecuted(EditEntityViewModel entity) => this.Entities.Remove(entity);

        private void RemoveEntity(Entity entity) => this.model.Entities.Delete(entity.Id);

        #endregion Delete Entity from Model

        #region Commit changes of Tags to model

        private void OnTagCommitted(EditTagViewModel tag) => this.OnTagCommitted(tag.Model);

        private void OnTagCommitted(Tag tag) => this.model.Tags.Upsert(tag);

        private void OnTagRemoved(EditTagViewModel vm) => this.model.Tags.Delete(vm.Model.Id);

        private void OnEntityCommitted(EditEntityViewModel entity) => this.OnEntityCommitted(entity.Model);

        private void OnEntityCommitted(Entity entity) => this.model.Entities.Upsert(entity);

        private void OnEntityRemoved(EditEntityViewModel vm) => this.model.Entities.Delete(vm.Model.Id);

        public void Commit()
        {
            this.tags.Value.Commit(onAdd: this.OnTagCommitted, onRemove: this.OnTagRemoved);
            this.entities.Value.Commit(onAdd: this.OnEntityCommitted, onRemove: this.OnEntityRemoved);
        }

        public void Rollback()
        {
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
        }

        private void CreateLazyTagsCollection()
        {
            this.tags = new Lazy<CommitableObservableCollection<EditTagViewModel>>(() => new CommitableObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.OnTagCommitted))));
            this.RaisePropertyChanged(nameof(Tags));
        }

        private void CreateLazyEntitiesCollection()
        {
            this.entities = new Lazy<CommitableObservableCollection<EditEntityViewModel>>(() => new CommitableObservableCollection<EditEntityViewModel>(this.model.Entities.FindAll().Select(e => new EditEntityViewModel(e, this.OnEntityCommitted))));
            this.RaisePropertyChanged(nameof(Entities));
        }

        #endregion Commit changes of Tags to model
    }
}