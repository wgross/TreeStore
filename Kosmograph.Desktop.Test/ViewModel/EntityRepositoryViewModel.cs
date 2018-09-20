using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EntityRepositoryViewModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IEntityRepository> model;
        private EntityRepositoryViewModel viewModel;

        public EntityRepositoryViewModelTest()
        {
            this.model = this.mocks.Create<IEntityRepository>();
            this.viewModel = new EntityRepositoryViewModel(this.model.Object, t => new TagViewModel(t));
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void EntityRepositoryViewModel_mirrors_Model()
        {
            // ARRANGE

            var entity = new Entity("e");
            this.model
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            // ACT

            this.viewModel.FillAll();

            // ASSERT

            Assert.Equal(entity, this.viewModel.Single().Model);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void EntityRepositoryViewModel_delays_created_Entity_to_KosmographModel()
        {
            // ACT

            this.viewModel.CreateCommand.Execute(null);

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Equal("new entity", this.viewModel.Edited.Name);
        }

        [Fact]
        public void EntityRepositoryViewModel_commits_created_Entity_to_KosmographModel()
        {
            // ARRANGE

            this.model
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Returns<Entity>(e => e);

            // create new entity
            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(this.viewModel);
            Assert.Equal("new entity", this.viewModel.Single().Name);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void EntityRepositoryViewModel_reverts_created_Entity_at_KosmographViewModel()
        {
            // ARRANGE

            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.RollbackCommand.Execute(null);

            // ASSERT
            // entiy is forgotton

            Assert.Empty(this.viewModel);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void EntityRepositoryViewModel_commits_deleted_Entity_to_Model()
        {
            // ARRANGE

            var entity = new Entity("e");
            this.model
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.model
                .Setup(r => r.Delete(entity.Id))
                .Returns(true);

            this.viewModel.FillAll();

            // ACT

            // remove tag
            this.viewModel.DeleteCommand.Execute(this.viewModel.Single());

            // ASSERT
            // tag is inserted in the list after commit

            Assert.Empty(this.viewModel);
            //Assert.Null(this.viewModel.SelectedEntity);
            //Assert.Null(this.viewModel.EditedEntity);
        }
    }
}