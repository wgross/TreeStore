using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Model;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class KosmographViewModel : ViewModelBase
    {
        private KosmographModel model;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.Tags = new TagRepositoryViewModel(this.model.Tags);
            this.Entities = new EntityRepositoryViewModel(this.model.Entities);
            this.Relationships = new RelationshipRepositoryViewModel(this.model.Relationships, this.Entities.GetViewModel);

            this.CreateTagCommand = new RelayCommand(this.CreateTagExecuted);
            this.EditTagCommand = new RelayCommand<TagViewModel>(this.EditTagExecuted);

            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.EditEntityCommand = new RelayCommand<EntityViewModel>(this.EditEntityExecuted);

            this.CreateRelationshipCommand = new RelayCommand(this.CreateRelationshipExecuted);
            this.EditRelationshipCommand = new RelayCommand<RelationshipViewModel>(this.EditRelationshipExecuted);
            this.Rollback();
        }

        public void FillAll()
        {
            this.Tags.FillAll();
            this.Entities.FillAll();
            this.Relationships.FillAll();
        }

        public KosmographModel Model => this.model;

        public TagRepositoryViewModel Tags { get; }

        public EntityRepositoryViewModel Entities { get; }

        public RelationshipRepositoryViewModel Relationships { get; }

        public TagViewModel SelectedTag
        {
            get => this.selectedTag;
            set => this.Set(nameof(SelectedTag), ref this.selectedTag, value);
        }

        private TagViewModel selectedTag;

        public EntityViewModel SelectedEntity
        {
            get => this.selectedEntity;
            set => this.Set(nameof(SelectedEntity), ref this.selectedEntity, value);
        }

        private EntityViewModel selectedEntity;

        public RelationshipViewModel SelectedRelationship
        {
            get => this.selectedRelationship;
            set => this.Set(nameof(SelectedRelationship), ref this.selectedRelationship, value);
        }

        public RelationshipViewModel selectedRelationship;

        #region Create new Tag in model

        public ICommand CreateTagCommand { get; }

        private void CreateTagExecuted()
        {
            this.EditedTag = new TagEditModel(
                new TagViewModel(new Tag("new tag", new Facet())),
                this.OnCreatedTagCommitted, this.OnTagRollback);
        }

        private void OnCreatedTagCommitted(Tag tag)
        {
            var tagViemModel = new TagViewModel(tag);
            this.Tags.Add(tagViemModel);
            //this.tags.Commit(onAdd: tvm => this.Model.Tags.Upsert(tvm.Model));
            this.EditedTag = null;
            this.SelectedTag = tagViemModel;
        }

        #endregion Create new Tag in model

        #region Edit existing Tag

        public ICommand EditTagCommand { get; }

        private void EditTagExecuted(TagViewModel tag)
        {
            this.EditedTag = new TagEditModel(tag, this.OnEditedTagCommitted, this.OnTagRollback);
        }

        private void OnEditedTagCommitted(Tag tag)
        {
            this.model.Tags.Upsert(tag);
            this.EditedTag = null;
        }

        private void OnTagRollback(Tag tag)
        {
            this.EditedTag = null;
        }

        #endregion Edit existing Tag

        public TagEditModel EditedTag
        {
            get => this.editedTag;
            set => this.Set<TagEditModel>(nameof(EditedTag), ref this.editedTag, value);
        }

        private TagEditModel editedTag;

        #region Create new Entity in Model

        public ICommand CreateEntityCommand { get; }

        public void CreateEntityExecuted()
        {
            this.EditedEntity = new EntityEditModel(new EntityViewModel(new Entity("new entity")), this.OnCreatedEntityCommitted, this.OnEntityRollback);
        }

        private void OnCreatedEntityCommitted(Entity entity)
        {
            var entityViewModel = new EntityEditModel(new EntityViewModel(entity), this.OnEditedEntityCommitted, this.OnEntityRollback);
            this.Entities.Add(entityViewModel.ViewModel);
            this.EditedEntity = null;
            this.SelectedEntity = entityViewModel.ViewModel;
        }

        #endregion Create new Entity in Model

        #region Edit existing Entity

        public ICommand EditEntityCommand { get; }

        private void EditEntityExecuted(EntityViewModel entity)
        {
            this.EditedEntity = new EntityEditModel(entity, this.OnEditedEntityCommitted, this.OnEntityRollback);
        }

        private void OnEditedEntityCommitted(Entity entity)
        {
            this.Model.Entities.Upsert(entity);
            this.EditedEntity = null;
        }

        private void OnEntityRollback(Entity entity)
        {
            this.EditedEntity = null;
        }

        #endregion Edit existing Entity

        public EntityEditModel EditedEntity

        {
            get => this.editedEntity;
            set => this.Set<EntityEditModel>(nameof(EditedEntity), ref this.editedEntity, value);
        }

        private EntityEditModel editedEntity;

        #region Create/Edit Relationship

        public ICommand CreateRelationshipCommand { get; set; }

        private void CreateRelationshipExecuted()
        {
            //this.EditedRelationship = new RelationshipEditModel(
            //    new RelationshipViewModel(new Relationship("new relationship")),
            //    this.OnCreatedRelationshipCommitted, this.OnRelationshipRollback);
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
            //var relationshipViewModel = new RelationshipViewModel(relationship);
            //this.Relationships.Add(relationshipViewModel);
            //this.EditedRelationship = null;
            //this.SelectedRelationship = relationshipViewModel;
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

        #region Commit changes of Tags to model

        private void OnTagRemoved(TagEditModel vm) => this.model.Tags.Delete(vm.ViewModel.Model.Id);

        private void OnEntityRemoved(EntityEditModel vm) => this.model.Entities.Delete(vm.ViewModel.Model.Id);

        public void Commit()
        {
            //is.tags.Value.Commit(onAdd: this.OnTagCommitted, onRemove: this.OnTagRemoved);
            //this.entities.Value.Commit(onAdd: this.OnEditedEntityCommitted, onRemove: this.OnEntityRemoved);
        }

        public void Rollback()
        {
        }

        #endregion Commit changes of Tags to model
    }
}
;