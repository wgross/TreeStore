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
            this.viewModel = new RelationshipRepositoryViewModel(this.model.Object, e => new EntityViewModel(e));
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

        //[Fact]
        //public void KosmographViewModel_delays_created_Relationship_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Relationships)
        //      .Returns(this.relationshipRepository.Object);

        //    var relationship = new Relationship("r");
        //    this.relationshipRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(relationship.Yield());

        //    // ACT

        //    this.viewModel.CreateRelationshipCommand.Execute(null);

        //    // ASSERT

        //    Assert.Single(this.viewModel.Relationships);
        //    Assert.Equal("new relationship", this.viewModel.EditedRelationship.Name);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_created_Relationship_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Relationships)
        //      .Returns(this.relationshipRepository.Object);

        //    var relationship = new Relationship("r", new Entity(), new Entity());
        //    this.relationshipRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(relationship.Yield());

        //    this.relationshipRepository
        //        .Setup(r => r.Upsert(It.IsAny<Relationship>()))
        //        .Returns<Relationship>(r => r);

        //    this.viewModel.CreateRelationshipCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedRelationship.CommitCommand.Execute(null);

        //    // ASSERT

        //    Assert.Equal(2, this.viewModel.Relationships.Count);
        //    Assert.Null(this.viewModel.EditedRelationship);
        //    Assert.Equal("new relationship", this.viewModel.Relationships.ElementAt(1).Name);
        //    Assert.Equal(this.viewModel.Relationships.ElementAt(1), this.viewModel.SelectedRelationship);
        //}

        [Fact]
        public void RelationshipRepositoryViewModel_deleted_Relationship_at_Model()
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
            //Assert.Null(this.viewModel.SelectedRelationship);
            //Assert.Null(this.viewModel.EditedRelationship);
        }
    }
}