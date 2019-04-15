using Kosmograph.Model;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public partial class KosmographViewModel
    {
        public Lists.ViewModel.RelationshipRepositoryViewModel Relationships { get; }

        public Lists.ViewModel.RelationshipViewModel SelectedRelationship
        {
            get => this.selectedRelationship;
            set => this.Set(nameof(SelectedRelationship), ref this.selectedRelationship, value);
        }

        public Lists.ViewModel.RelationshipViewModel selectedRelationship;

        #region Edit Relatsionhsip

        public ICommand EditRelationshipCommand { get; }

        private void EditRelationshipExecuted(Lists.ViewModel.RelationshipViewModel entity) => this.EditedRelationship = new Editors.ViewModel.RelationshipEditModel(entity.Model, this.EditRelationshipCommitted, this.EditRelationshipRollback);

        private Editors.ViewModel.RelationshipEditModel editedRelationship;

        public Editors.ViewModel.RelationshipEditModel EditedRelationship
        {
            get => this.editedRelationship;
            set => this.Set(nameof(EditedRelationship), ref this.editedRelationship, value);
        }

        private void EditRelationshipCommitted(Relationship relationship)
        {
            this.model.Relationships.Upsert(relationship);
            this.EditedRelationship = null;
        }

        private void EditRelationshipRollback(Relationship obj) => this.EditedRelationship = null;

        #endregion Edit Relatsionhsip

        #region Create Relationship

        public ICommand CreateRelationshipCommand { get; }

        private void CreateRelationshipExecuted()
            => this.EditedRelationship = new Editors.ViewModel.RelationshipEditModel(new Relationship("new relationship"), this.CreateRelationshipCommitted, this.CreateRelationshipRollback);

        private void CreateRelationshipRollback(Relationship relationship) => this.EditedRelationship = null;

        private void CreateRelationshipCommitted(Relationship relationship)
        {
            this.model.Relationships.Upsert(relationship);
            this.EditedRelationship = null;
        }

        #endregion Create Relationship
    }
}