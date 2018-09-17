using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
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

            this.CreateRelationshipCommand = new RelayCommand(this.CreateRelationshipExecuted);
            this.EditRelationshipCommand = new RelayCommand<RelationshipViewModel>(this.EditRelationshipExecuted);
            this.DeleteRelationshipCommand = new RelayCommand<RelationshipViewModel>(this.DeleteRelationshipExecuted);
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

        public RelationshipViewModel SelectedRelationship
        {
            get => this.selectedRelationship;
            set => this.Set(nameof(SelectedRelationship), ref this.selectedRelationship, value);
        }

        public RelationshipViewModel selectedRelationship;

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
            this.EditedEntity = new EditEntityViewModel(new Entity("new entity"), this.OnCreatedEntityCommitted, this.OnEntityRollback);
        }

        private void OnCreatedEntityCommitted(Entity entity)
        {
            var entityViewModel = new EditEntityViewModel(entity, this.OnEditedEntityCommitted, this.OnEntityRollback);
            this.entities.Value.Add(entityViewModel);
            this.entities.Value.Commit(onAdd: evm => this.Model.Entities.Upsert(evm.Model));
            this.EditedEntity = null;
            this.SelectedEntity = entityViewModel;
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

        #region Create/Edit Relationship

        public ICommand CreateRelationshipCommand { get; set; }

        private void CreateRelationshipExecuted()
        {
            this.EditedRelationship = new RelationshipEditModel(
                new RelationshipViewModel(new Relationship("new relationship")),
                this.OnCreatedRelationshipCommitted, this.OnRelationshipRollback);
        }

        public ICommand EditRelationshipCommand { get; }

        private void EditRelationshipExecuted(RelationshipViewModel viewModel)
        {
            this.EditedRelationship = new RelationshipEditModel(viewModel, this.OnEditedRelationshipCommitted, this.OnRelationshipRollback);
        }

        public RelationshipEditModel EditedRelationship
        {
            get => this.editedRelationship;
            set => this.Set(nameof(EditedRelationship), ref this.editedRelationship, value);
        }

        private RelationshipEditModel editedRelationship;

        private void OnCreatedRelationshipCommitted(Relationship relationship)
        {
            var relationshipViewModel = new RelationshipViewModel(relationship);
            this.relationships.Value.Add(relationshipViewModel);
            this.relationships.Value.Commit(onAdd: rvm => this.Model.Relationships.Upsert(rvm.Model));
            this.EditedRelationship = null;
            this.SelectedRelationship = relationshipViewModel;
        }

        private void OnEditedRelationshipCommitted(Relationship entity)
        {
            this.Model.Relationships.Upsert(entity);
            this.EditedRelationship = null;
        }

        private void OnRelationshipRollback(Relationship obj)
        {
            this.EditedRelationship = null;
        }

        #endregion Create/Edit Relationship

        #region Deleted Relationship from mode

        public ICommand DeleteRelationshipCommand { get; }

        private void DeleteRelationshipExecuted(RelationshipViewModel relationship)
        {
            this.relationships.Value.Remove(relationship);
            this.relationships.Value.Commit(onRemove: rvm => this.model.Relationships.Delete(rvm.Model.Id));
        }

        #endregion Deleted Relationship from mode

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