using Kosmograph.Model;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public partial class KosmographViewModel
    {
        public Lists.ViewModel.EntityRepositoryViewModel Entities { get; }

        public Lists.ViewModel.EntityViewModel SelectedEntity
        {
            get => this.selectedEntity;
            set => this.Set(nameof(SelectedEntity), ref this.selectedEntity, value);
        }

        private Lists.ViewModel.EntityViewModel selectedEntity;

        #region Edit Entity

        //private sealed class EntityEditCallbackHandler : Editors.ViewModel.IEntityEditCallback
        //{
        //    private readonly IEntityRepository tagRepository;
        //    private readonly Action<Entity> onCommit;
        //    private readonly Action<Entity> onRollback;

        //    public EntityEditCallbackHandler(IEntityRepository tagRepository, Action<Entity> onCommit, Action<Entity> onRollback)
        //    {
        //        this.tagRepository = tagRepository;
        //        this.onCommit = onCommit;
        //        this.onRollback = onRollback;
        //    }

        //    public bool HasError { get; private set; }

        //    public bool CanCommit(Editors.ViewModel.EntityEditModel tag) => !this.HasError;

        //    public void Commit(Entity tag) => this.onCommit(tag);

        //    public void Rollback(Entity tag) => this.onRollback(tag);

        //    public string Validate(Editors.ViewModel.EntityEditModel tag)
        //    {
        //        var possibleDuplicate = this.tagRepository.FindByName(tag.Name);
        //        if (possibleDuplicate is null || possibleDuplicate.Equals(tag.Model))
        //        {
        //            this.HasError = false;
        //            return null;
        //        }
        //        else
        //        {
        //            this.HasError = true;
        //            return "Entity name must be unique";
        //        }
        //    }
        //}

        public ICommand EditEntityCommand { get; }

        private Editors.ViewModel.EntityEditModel editedEntity;

        public Editors.ViewModel.EntityEditModel EditedEntity
        {
            get => this.editedEntity;
            set => this.Set(nameof(EditedEntity), ref this.editedEntity, value);
        }

        private void EditEntityExecuted(Lists.ViewModel.EntityViewModel entity) => this.EditedEntity = new Editors.ViewModel.EntityEditModel(entity.Model, this.EditEntityCommitted, this.EditEntityRollback);

        private void EditEntityRollback(Entity _) => this.EditedEntity = null;

        private void EditEntityCommitted(Entity entity)
        {
            this.model.Entities.Upsert(entity);
            this.EditedEntity = null;
        }

        #endregion Edit Entity

        #region Create Entity

        public ICommand CreateEntityCommand { get; set; }

        private void CreateEntityExecuted() => this.EditedEntity = new Editors.ViewModel.EntityEditModel(new Entity("new entity"), this.CreateEntityCommitted, this.CreateEntityRollback);

        private void CreateEntityRollback(Entity entity) => this.EditedEntity = null;

        private void CreateEntityCommitted(Entity entity)
        {
            this.model.Entities.Upsert(entity);
            this.EditedEntity = null;
        }

        #endregion Create Entity
    }
}