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

        #region Remove Tag from model

        public ICommand DeleteTagCommand { get; }

        private void DeleteTagExecuted(EditTagViewModel tag)
        {
            this.tags.Value.Remove(tag);
            this.tags.Value.Commit(onRemove: tvm => this.model.Tags.Delete(tvm.Model.Id));
        }

        #endregion Remove Tag from model

        #region Create new Tag in model

        public ICommand CreateTagCommand { get; }

        private void CreateTagExecuted()
        {
            this.EditedTag = new EditTagViewModel(new Tag("new tag", new Facet()), this.OnCreatedTagCommitted, this.OnTagRollback);
        }

        private void OnCreatedTagCommitted(Tag tag)
        {
            if (this.EditedTag.Model.Equals(tag))
                this.EditedTag = null;

            var tagViemModel = new EditTagViewModel(tag, this.OnEditedTagCommitted, this.OnTagRollback);
            this.tags.Value.Add(tagViemModel);
            this.tags.Value.Commit(onAdd: tvm => this.model.Tags.Upsert(tvm.Model));
            this.SelectedTag = tagViemModel;
        }

        #endregion Create new Tag in model

        #region Edit existing Tag

        public ICommand EditTagCommand { get; }

        private void EditTagExecuted(EditTagViewModel tag)
        {
            this.EditedTag = tag;
        }

        private void OnEditedTagCommitted(Tag tag)
        {
            if (this.EditedTag.Model.Equals(tag))
                this.EditedTag = null;

            this.model.Tags.Upsert(tag);
        }

        private void OnTagRollback(Tag tag)
        {
            if (this.EditedTag.Model.Equals(tag))
                this.EditedTag = null;
        }

        #endregion Edit existing Tag

        public EditTagViewModel EditedTag
        {
            get => this.editedTag;
            set => this.Set<EditTagViewModel>(nameof(EditedTag), ref this.editedTag, value);
        }

        private EditTagViewModel editedTag;

        #region Create new Entity in Model

        public ICommand CreateEntityCommand { get; }

        public void CreateEntityExecuted()
        {
            this.EditedEntity = new EditEntityViewModel(new Entity("new entity", new Tag(string.Empty, new Facet())), this.OnCreatedEntityCommitted, this.OnEntityRollback);
        }

        private void OnCreatedEntityCommitted(Entity entity)
        {
            if (this.EditedEntity.Model.Equals(entity))
                this.EditedEntity = null;

            var entityViemModel = new EditEntityViewModel(entity, this.OnEditedEntityCommitted, this.OnEntityRollback);
            this.entities.Value.Add(entityViemModel);
            this.entities.Value.Commit(onAdd: evm => this.model.Entities.Upsert(evm.Model));
            this.SelectedEntity = entityViemModel;
        }

        #endregion Create new Entity in Model

        #region Edit existing Entity

        public ICommand EditEntityCommand { get; }

        private void EditEntityExecuted(EditEntityViewModel entity)
        {
            this.EditedEntity = entity;
        }

        private void OnEditedEntityCommitted(Entity entity)
        {
            if (this.EditedEntity.Model.Equals(entity))
                this.EditedEntity = null;

            this.model.Entities.Upsert(entity);
        }

        private void OnEntityRollback(Entity entity)
        {
            if (this.EditedEntity.Model.Equals(entity))
                this.EditedEntity = null;
        }

        #endregion Edit existing Entity

        public EditEntityViewModel EditedEntity

        {
            get => this.editedEntity;
            set => this.Set<EditEntityViewModel>(nameof(EditedEntity), ref this.editedEntity, value);
        }

        private EditEntityViewModel editedEntity;

        #region Delete Entity from Model

        public ICommand DeleteEntityCommand { get; }

        private void DeleteEntityExecuted(EditEntityViewModel entity)
        {
            this.entities.Value.Remove(entity);
            this.entities.Value.Commit(onRemove: evm => this.model.Entities.Delete(evm.Model.Id));
        }

        #endregion Delete Entity from Model

        #region Commit changes of Tags to model

        private void OnTagRemoved(EditTagViewModel vm) => this.model.Tags.Delete(vm.Model.Id);

        private void OnEntityRemoved(EditEntityViewModel vm) => this.model.Entities.Delete(vm.Model.Id);

        public void Commit()
        {
            //is.tags.Value.Commit(onAdd: this.OnTagCommitted, onRemove: this.OnTagRemoved);
            //this.entities.Value.Commit(onAdd: this.OnEditedEntityCommitted, onRemove: this.OnEntityRemoved);
        }

        public void Rollback()
        {
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
        }

        private void CreateLazyTagsCollection()
        {
            this.tags = new Lazy<CommitableObservableCollection<EditTagViewModel>>(() => new CommitableObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.OnEditedTagCommitted, this.OnTagRollback))));
            this.RaisePropertyChanged(nameof(Tags));
        }

        private void CreateLazyEntitiesCollection()
        {
            this.entities = new Lazy<CommitableObservableCollection<EditEntityViewModel>>(() =>
                new CommitableObservableCollection<EditEntityViewModel>(this.model.Entities.FindAll().Select(e => new EditEntityViewModel(e, this.OnEditedEntityCommitted, this.OnEntityRollback))));
            this.RaisePropertyChanged(nameof(Entities));
        }

        #endregion Commit changes of Tags to model
    }
}