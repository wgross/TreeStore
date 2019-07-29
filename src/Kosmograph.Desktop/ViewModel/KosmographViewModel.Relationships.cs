using Kosmograph.Model;
using System;
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

        public ICommand EditRelationshipByIdCommand { get; }

        private void EditRelationshipByIdExecuted(Guid relationshipId) => this.EditRelationship(this.Model.Relationships.FindById(relationshipId));

        private void EditRelationship(Relationship relationship) => this.EditedRelationship = new Editors.ViewModel.RelationshipEditModel(relationship, this.EditRelationshipCommitted, this.EditRelationshipRollback);

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

        #region Delete Relationship

        public ICommand DeleteRelationshipByIdCommand { get; set; }

        private void DeleteRelationshipByIdExecuted(Guid relationshipId) => this.DeleteRelationship(this.Model.Relationships.FindById(relationshipId));

        private void DeleteRelationship(Relationship relationship) => this.Model.Relationships.Delete(relationship);

        #endregion Delete Relationship
    }
}