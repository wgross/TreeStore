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
            this.EditTagCommand = new RelayCommand<EditTagViewModel>(this.EditTagExecuted);
            this.DeleteTagCommand = new RelayCommand<EditTagViewModel>(this.DeleteTagExecuted);

            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.EditEntityCommand = new RelayCommand<EditEntityViewModel>(this.EditEntityExecuted);
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

        #region Create/edit new Tag in model

        public ICommand CreateTagCommand { get; }

        private void CreateTagExecuted()
        {
            var tmp = new EditTagViewModel(new Tag("new tag", new Facet()), this.OnTagCommitted);
            this.Tags.Add(tmp);
            this.EditTagCommand.Execute(tmp);
        }

        public ICommand EditTagCommand { get; }

        private void EditTagExecuted(EditTagViewModel tag)
        {
            this.EditedTag = tag;
        }

        public EditTagViewModel EditedTag
        {
            get => this.editedTag;
            set => this.Set<EditTagViewModel>(nameof(EditedTag), ref this.editedTag, value);
        }

        private EditTagViewModel editedTag;

        #endregion Create/edit new Tag in model

        #region Create/edit Entity in Model

        public ICommand CreateEntityCommand { get; }

        public void CreateEntityExecuted()
        {
            var tmp = new EditEntityViewModel(new Entity("new entity", new Tag(string.Empty, new Facet())), this.OnEntityCommitted);
            this.Entities.Add(tmp);
            this.EditEntityCommand.Execute(tmp);
            this.SelectedEntity = tmp;
        }

        public ICommand EditEntityCommand { get; }

        private void EditEntityExecuted(EditEntityViewModel entity)
        {
            this.EditedEntity = entity;
        }

        public EditEntityViewModel EditedEntity
        {
            get => this.editedEntity;
            set => this.Set<EditEntityViewModel>(nameof(EditedEntity), ref this.editedEntity, value);
        }

        private EditEntityViewModel editedEntity;

        #endregion Create/edit Entity in Model

        #region Delete Entity from Model

        public ICommand DeleteEntityCommand { get; }

        private void DeleteEntityExecuted(EditEntityViewModel entity) => this.Entities.Remove(entity);

        private void RemoveEntity(Entity entity) => this.model.Entities.Delete(entity.Id);

        #endregion Delete Entity from Model

        #region Commit changes of Tags to model

        private void OnTagCommitted(EditTagViewModel tag)
        {
            if (tag.Equals(this.EditedTag))
                this.EditedTag = null;

            this.OnTagCommitted(tag.Model);
        }

        private void OnTagCommitted(Tag tag) => this.model.Tags.Upsert(tag);

        private void OnTagRemoved(EditTagViewModel vm) => this.model.Tags.Delete(vm.Model.Id);

        private void OnEntityCommitted(EditEntityViewModel entity)
        {
            if (entity.Equals(this.EditedEntity))
                this.EditedEntity = null;

            this.OnEntityCommitted(entity.Model);
        }

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