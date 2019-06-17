using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelRelationshipsTest : KosmographViewModelTestBase
    {
        private Lists.ViewModel.RelationshipViewModel DefaultRelationhipViewModel(Action<RelationshipViewModel> setup = null)
            => Setup(new Lists.ViewModel.RelationshipViewModel(DefaultRelationship()));

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_Relationship()
        {
            // ARRANGE

            var relationshipViewModel = DefaultRelationhipViewModel();

            // ACT
            // start editing of relationship

            this.ViewModel.EditRelationshipCommand.Execute(relationshipViewModel);

            // ASSERT
            // editor weas created

            Assert.NotNull(this.ViewModel.EditedRelationship);
            Assert.Equal(relationshipViewModel.Model, this.ViewModel.EditedRelationship.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_existing_relationship_on_commit()
        {
            // ARRANGE
            // edit a relationship

            var relationshipViewModel = DefaultRelationhipViewModel();
            this.ViewModel.EditRelationshipCommand.Execute(relationshipViewModel);
            this.RelationshipRepository
                .Setup(r => r.Upsert(relationshipViewModel.Model))
                .Returns(relationshipViewModel.Model);

            // ACT
            // committing the relationship upserts the relationship to the db

            this.ViewModel.EditedRelationship.CommitCommand.Execute(null);

            // ASSERT
            // the relatsionhip was sent to repo

            Assert.Null(this.ViewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_relationship_on_rollback()
        {
            // ARRANGE
            // edit a relationship

            var relationshipViewModel = DefaultRelationhipViewModel();
            this.ViewModel.EditRelationshipCommand.Execute(relationshipViewModel);

            // ACT
            // rollback the relationship

            this.ViewModel.EditedRelationship.RollbackCommand.Execute(null);

            // ASSERT
            // the relationship is sent to the repo

            Assert.Null(this.ViewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Relationship()
        {
            // ACT
            // start editing of relationship

            this.ViewModel.CreateRelationshipCommand.Execute(null);

            // ASSERT
            // editor weas created

            Assert.NotNull(this.ViewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_saves_new_relationship_on_commit()
        {
            // ARRANGE
            // edit a relationship

            this.ViewModel.CreateRelationshipCommand.Execute(null);
            this.RelationshipRepository
                .Setup(r => r.Upsert(It.IsAny<Relationship>()))
                .Returns<Relationship>(r => r);

            // ACT
            // committing the relationship upserts the relationship to the db

            this.ViewModel.EditedRelationship.From = DefaultEntity();
            this.ViewModel.EditedRelationship.To = DefaultEntity();
            this.ViewModel.EditedRelationship.CommitCommand.Execute(null);

            // ASSERT
            // the relatsionhip was sent to repo

            Assert.Null(this.ViewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_clears_new_relationship_on_rollback()
        {
            // ARRANGE

            this.ViewModel.CreateRelationshipCommand.Execute(null);

            // ACT
            // rollback editor

            this.ViewModel.EditedRelationship.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.ViewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographVIewModel_deletes_relationship_at_model()
        {
            // ARRANGE

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.Delete(relationship))
                .Returns(true);

            // ACT

            this.ViewModel.DeleteRelationshipCommand.Execute(new RelationshipViewModel(relationship));
        }
    }
}