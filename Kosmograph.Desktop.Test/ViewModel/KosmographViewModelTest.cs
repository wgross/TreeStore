using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Collections.Generic;
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

            // ACT
            // create new model item and remove existing

            this.viewModel.CreateTagCommand.Execute(null);
            this.viewModel.CreateEntityCommand.Execute(null);
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());
            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ASSERT
            // change of collection doesn't trigger DB update

            Assert.NotNull(this.viewModel.SelectedTag);
            Assert.Single(this.viewModel.Tags);
            Assert.Contains(this.viewModel.SelectedTag, this.viewModel.Tags);
            Assert.Equal("new tag", this.viewModel.Tags.Single().Name);

            Assert.NotNull(this.viewModel.SelectedEntity);
            Assert.Single(this.viewModel.Entities);
            Assert.Contains(this.viewModel.SelectedEntity, this.viewModel.Entities);
            Assert.Equal("new entity", this.viewModel.Entities.Single().Name);
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

            // create new model items and remove existing items
            this.viewModel.CreateTagCommand.Execute(null);
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());
            this.viewModel.CreateEntityCommand.Execute(null);
            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ACT

            this.viewModel.Commit();

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal("new tag", this.viewModel.Tags.Single().Name);
            Assert.Single(this.viewModel.Entities);
            Assert.Equal("new entity", this.viewModel.Entities.Single().Name);
        }

        [Fact]
        public void KosmographViewModel_reverts_changes_from_KosmographModel()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.viewModel.CreateTagCommand.Execute(null);

            var entity = new Entity("e", tag);
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.viewModel.CreateTagCommand.Execute(null);
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            this.viewModel.CreateEntityCommand.Execute(null);
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

        [Fact]
        public void KosmographViewModel_raise_property_chaned_on_selected_item_change()
        {
            // ARRANGE

            var tag = new Tag();

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            var editTag = this.viewModel.Tags.Single();

            var entity = new Entity();

            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            var editEntity = this.viewModel.Entities.Single();

            var properties = new List<string>();
            this.viewModel.PropertyChanged += (s, e) => properties.Add(e.PropertyName);

            // ACT

            this.viewModel.SelectedEntity = editEntity;
            this.viewModel.SelectedTag = editTag;

            // ASSERT

            Assert.Equal(new[] { nameof(KosmographViewModel.SelectedEntity), nameof(KosmographViewModel.SelectedTag) }, properties);
        }
    }
}