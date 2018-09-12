using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IKosmographPersistence> persistence;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly Mock<IEntityRepository> entityRepository;
        private readonly KosmographViewModel viewModel;

        public KosmographViewModelTest()
        {
            this.persistence = this.mocks.Create<IKosmographPersistence>(MockBehavior.Loose);
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.entityRepository = this.mocks.Create<IEntityRepository>();
            this.persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);
            this.persistence
                .Setup(p => p.Entities)
                .Returns(this.entityRepository.Object);
            this.viewModel = new KosmographViewModel(new KosmographModel(this.persistence.Object));
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void KosmographViewModel_mirrors_KosmographModel()
        {
            // ARRANGE

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(new Tag().Yield());

            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(new Entity().Yield());

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Single(this.viewModel.Entities);
        }

        [Fact]
        public void KosmographViewModel_delays_changes_at_KosmographModel()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            var entity = new Entity("e", new Tag());
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            var editTag = this.viewModel.CreateNewTag();
            var editEntity = this.viewModel.CreateNewEntity();

            // ACT

            this.viewModel.Tags.Add(editTag);
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());
            this.viewModel.Entities.Add(editEntity);
            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ASSERT
            // change of collection doesn't trigger DB update

            Assert.Single(this.viewModel.Tags);
            Assert.Equal(string.Empty, this.viewModel.Tags.Single().Name);
            Assert.Single(this.viewModel.Entities);
            Assert.Equal(string.Empty, this.viewModel.Entities.Single().Name);
        }

        [Fact]
        public void KosmographViewModel_commits_changes_at_KosmographModel()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.tagRepository
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Returns<Tag>(t => t);

            this.tagRepository
                .Setup(r => r.Delete(tag.Id))
                .Returns(true);

            var editTag = this.viewModel.CreateNewTag();

            var entity = new Entity("e", tag);
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.entityRepository
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Returns<Entity>(e => e);

            this.entityRepository
                .Setup(r => r.Delete(entity.Id))
                .Returns(true);

            this.viewModel.Tags.Add(this.viewModel.CreateNewTag());
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            this.viewModel.Entities.Add(this.viewModel.CreateNewEntity());
            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ACT

            this.viewModel.Commit();

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal(string.Empty, this.viewModel.Tags.Single().Name);
            Assert.Single(this.viewModel.Entities);
            Assert.Equal(string.Empty, this.viewModel.Entities.Single().Name);
        }

        [Fact]
        public void KosmographViewModel_reverts_changes_from_KosmographModel()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            var editTag = this.viewModel.CreateNewTag();

            var entity = new Entity("e", tag);
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            var editEntity = this.viewModel.CreateNewEntity();

            this.viewModel.Tags.Add(this.viewModel.CreateNewTag());
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            this.viewModel.Entities.Add(this.viewModel.CreateNewEntity());
            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ACT

            this.viewModel.Rollback();

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal("t", this.viewModel.Tags.Single().Name);
            Assert.Single(this.viewModel.Entities);
            Assert.Equal("e", this.viewModel.Entities.Single().Name);
        }

        [Fact]
        public void KosmographViewModel_writes_modified_item_to_persistence()
        {
            // ARRANGE

            var tag = new Tag();

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.tagRepository
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            var editTag = this.viewModel.Tags.Single();

            var entity = new Entity();

            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.entityRepository
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            var editEntity = this.viewModel.Entities.Single();

            // ACT

            editTag.Name = "changed";
            editTag.Commit();
            editEntity.Name = "changed";
            editEntity.Commit();
        }
    }
}