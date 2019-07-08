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

            var relationship = DefaultRelationship();

            this.RelationshipRepository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            // ACT
            // start editing of relationship

            this.ViewModel.EditRelationshipByIdCommand.Execute(relationship.Id);

            // ASSERT
            // editor weas created

            Assert.NotNull(this.ViewModel.EditedRelationship);
            Assert.Equal(relationship, this.ViewModel.EditedRelationship.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_existing_relationship_on_commit()
        {
            // ARRANGE
            // edit a relationship

            var relationship = DefaultRelationship();

            this.RelationshipRepository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.ViewModel.EditRelationshipByIdCommand.Execute(relationship.Id);

            this.RelationshipRepository
                .Setup(r => r.Upsert(relationship))
                .Returns(relationship);

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

            var relationship = DefaultRelationship();

            this.RelationshipRepository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.ViewModel.EditRelationshipByIdCommand.Execute(relationship.Id);

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
        public void KosmographViewModel_deletes_relationship_at_model()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            this.RelationshipRepository
               .Setup(r => r.FindById(relationship.Id))
               .Returns(relationship);

            this.RelationshipRepository
                .Setup(r => r.Delete(relationship))
                .Returns(true);

            // ACT

            this.ViewModel.DeleteRelationshipByIdCommand.Execute(relationship.Id);
        }
    }
}