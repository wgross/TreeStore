using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelRelationshipsTest : KosmographViewModelTestBase
    {
        private readonly Mock<IRelationshipRepository> relationshipRepository;
        private readonly Mock<IEntityRepository> entityRepository;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly Mock<IKosmographPersistence> persistence;
        private readonly Desktop.ViewModel.KosmographViewModel viewModel;

        public KosmographViewModelRelationshipsTest()
        {
            this.relationshipRepository = this.mocks.Create<IRelationshipRepository>();
            this.entityRepository = this.mocks.Create<IEntityRepository>();
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.persistence = this.mocks.Create<IKosmographPersistence>();
            this.persistence
                .Setup(p => p.Entities)
                .Returns(this.entityRepository.Object);
            this.persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);
            this.persistence
                .Setup(p => p.Relationships)
                .Returns(this.relationshipRepository.Object);
            this.viewModel = new Kosmograph.Desktop.ViewModel.KosmographViewModel(new Model.KosmographModel(this.persistence.Object));
        }

        private Lists.ViewModel.RelationshipViewModel DefaultRelationhipViewModel(Action<RelationshipViewModel> setup = null)
            => Setup(new Lists.ViewModel.RelationshipViewModel(DefaultRelationship()));

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_Relationship()
        {
            // ARRANGE

            var relationshipViewModel = DefaultRelationhipViewModel();

            // ACT
            // start editing of relationship

            this.viewModel.EditRelationshipCommand.Execute(relationshipViewModel);

            // ASSERT
            // editor weas created

            Assert.NotNull(this.viewModel.EditedRelationship);
            Assert.Equal(relationshipViewModel.Model, this.viewModel.EditedRelationship.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_existing_relationship_on_commit()
        {
            // ARRANGE
            // edit a relationship

            var relationshipViewModel = DefaultRelationhipViewModel();
            this.viewModel.EditRelationshipCommand.Execute(relationshipViewModel);
            this.relationshipRepository
                .Setup(r => r.Upsert(relationshipViewModel.Model))
                .Returns(relationshipViewModel.Model);

            // ACT
            // committing the relationship upserts the relationship to the db

            this.viewModel.EditedRelationship.CommitCommand.Execute(null);

            // ASSERT
            // the relatsionhip was sent to repo

            Assert.Null(this.viewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_relationship_on_rollback()
        {
            // ARRANGE
            // edit a relationship

            var relationshipViewModel = DefaultRelationhipViewModel();
            this.viewModel.EditRelationshipCommand.Execute(relationshipViewModel);

            // ACT
            // rollback the relationship

            this.viewModel.EditedRelationship.RollbackCommand.Execute(null);

            // ASSERT
            // the relationship is sent to the repo

            Assert.Null(this.viewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Relationship()
        {
            // ACT
            // start editing of relationship

            this.viewModel.CreateRelationshipCommand.Execute(null);

            // ASSERT
            // editor weas created

            Assert.NotNull(this.viewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_saves_new_relationship_on_commit()
        {
            // ARRANGE
            // edit a relationship

            this.viewModel.CreateRelationshipCommand.Execute(null);
            this.relationshipRepository
                .Setup(r => r.Upsert(It.IsAny<Relationship>()))
                .Returns<Relationship>(r => r);

            // ACT
            // committing the relationship upserts the relationship to the db

            this.viewModel.EditedRelationship.From = DefaultEntity();
            this.viewModel.EditedRelationship.To = DefaultEntity();
            this.viewModel.EditedRelationship.CommitCommand.Execute(null);

            // ASSERT
            // the relatsionhip was sent to repo

            Assert.Null(this.viewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographViewModel_clears_new_relationship_on_rollback()
        {
            // ARRANGE

            this.viewModel.CreateRelationshipCommand.Execute(null);

            // ACT
            // rollback editor

            this.viewModel.EditedRelationship.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.viewModel.EditedRelationship);
        }

        [Fact]
        public void KosmographVIewModel_deletes_relationship_at_model()
        {
            // ARRANGE

            var relationship = DefaultRelationship();
            this.relationshipRepository
                .Setup(r => r.Delete(relationship))
                .Returns(true);

            // ACT

            this.viewModel.DeleteRelationshipCommand.Execute(new RelationshipViewModel(relationship));
        }
    }
}