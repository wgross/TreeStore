using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class RelationshipRepositoryViewModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IRelationshipRepository> model;
        private RelationshipRepositoryViewModel viewModel;

        public RelationshipRepositoryViewModelTest()
        {
            this.model = this.mocks.Create<IRelationshipRepository>();
            this.viewModel = new RelationshipRepositoryViewModel(this.model.Object,
                newEntityViewModel: e => new EntityViewModel(e),
                newTagViewModel: t => new TagViewModel(t));
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void RelationshipRepositoryViewModel_mirrors_Model()
        {
            // ARRANGE

            var relationship = new Relationship("r", new Entity(), new Entity());
            this.model
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            // ACT

            this.viewModel.FillAll();

            // ASSERT

            Assert.Single(this.viewModel);
            Assert.Equal(relationship, this.viewModel.Single().Model);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_creates_new_RelationshipViewModel()
        {
            // ARRANGE

            var relationship = new Relationship("r", new Entity(), new Entity());

            // ACT

            var result = this.viewModel.CreateViewModel(relationship);

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Equal(relationship, result.Model);
            Assert.Equal(relationship.From, result.From.Model);
            Assert.Equal(relationship.To, result.To.Model);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_delays_created_Relationship_to_KosmographModel()
        {
            // ACT

            this.viewModel.CreateCommand.Execute(null);

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Equal("new relationship", this.viewModel.Edited.Name);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_commits_created_Relationship_to_KosmographModel()
        {
            // ARRANGE

            this.model
                .Setup(r => r.Upsert(It.IsAny<Relationship>()))
                .Returns<Relationship>(r => r);

            //this.viewModel.FillAll()
            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(this.viewModel);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_reverts_created_Relationship_to_KosmographModel()
        {
            // ARRANGE

            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_deletes_Relationship_from_Model()
        {
            // ARRANGE

            var relationship = new Relationship("r", new Entity("e1"), new Entity("e2"));
            this.model
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.viewModel.FillAll();

            this.model
                .Setup(r => r.Delete(relationship.Id))
                .Returns(true);

            // ACT

            this.viewModel.DeleteCommand.Execute(this.viewModel.Single());

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Null(this.viewModel.Edited);
        }
    }
}