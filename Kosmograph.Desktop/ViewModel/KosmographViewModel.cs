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
        private Lazy<CommitableObservableCollection<RelationshipViewModel>> relationships;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditTagViewModel>)> changesAtTags;
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<EditEntityViewModel>)> changesAtEntities;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.CreateLazyTagsCollection();
            this.CreateLazyEntitiesCollection();
            this.CreateLazyRelationshipsCollection();

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

        private void CreateLazyTagsCollection()
        {
            this.tags = new Lazy<CommitableObservableCollection<EditTagViewModel>>(() => new CommitableObservableCollection<EditTagViewModel>(this.model.Tags.FindAll().Select(t => new EditTagViewModel(t, this.OnEditedTagCommitted, this.OnTagRollback))));
            this.RaisePropertyChanged(nameof(Tags));
        }

        public ObservableCollection<EditEntityViewModel> Entities => this.entities.Value;

        private void CreateLazyEntitiesCollection()
        {
            this.entities = new Lazy<CommitableObservableCollection<EditEntityViewModel>>(() =>
                new CommitableObservableCollection<EditEntityViewModel>(this.model.Entities.FindAll().Select(e => new EditEntityViewModel(e, this.OnEditedEntityCommitted, this.OnEntityRollback))));
            this.RaisePropertyChanged(nameof(Entities));
        }

        public ObservableCollection<RelationshipViewModel> Relationships => this.relationships.Value;

        private void CreateLazyRelationshipsCollection()
        {
            this.relationships = new Lazy<CommitableObservableCollection<RelationshipViewModel>>(() =>
                new CommitableObservableCollection<RelationshipViewModel>(this.model.Relationships.FindAll().Select(r => new RelationshipViewModel(r))));
            this.RaisePropertyChanged(nameof(Relationships));
        }

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
            var tagViemModel = new EditTagViewModel(tag, this.OnEditedTagCommitted, this.OnTagRollback);
            this.tags.Value.Add(tagViemModel);
            this.tags.Value.Commit(onAdd: tvm => this.Model.Tags.Upsert(tvm.Model));
            this.EditedTag = null;
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
            var entityViemModel = new EditEntityViewModel(entity, this.OnEditedEntityCommitted, this.OnEntityRollback);
            this.entities.Value.Add(entityViemModel);
            this.entities.Value.Commit(onAdd: evm => this.Model.Entities.Upsert(evm.Model));
            this.EditedEntity = null;
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
            this.Model.Entities.Upsert(entity);
            this.EditedEntity = null;
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

        public EditRelationshipViewModel EditedRelationship { get; set; }

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

        #endregion Commit changes of Tags to model
    }
}
;