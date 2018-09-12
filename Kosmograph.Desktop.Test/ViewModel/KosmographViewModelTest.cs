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
        private readonly KosmographViewModel viewModel;

        public KosmographViewModelTest()
        {
            this.persistence = this.mocks.Create<IKosmographPersistence>();
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);
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

            // ASSERT

            Assert.Single(this.viewModel.Tags);
        }

        [Fact]
        public void KosmographViewModel_delays_changes_at_KosmographModel()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            var editTag = this.viewModel.CreateNewTag();

            // ACT

            this.viewModel.Tags.Add(this.viewModel.CreateNewTag());
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal(string.Empty, this.viewModel.Tags.Single().Name);
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

            this.viewModel.Tags.Add(this.viewModel.CreateNewTag());
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            // ACT

            this.viewModel.Commit();

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal(string.Empty, this.viewModel.Tags.Single().Name);
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

            this.viewModel.Tags.Add(this.viewModel.CreateNewTag());
            this.viewModel.DeleteTagCommand.Execute(this.viewModel.Tags.First());

            // ACT

            this.viewModel.Rollback();

            // ASSERT

            Assert.Single(this.viewModel.Tags);
            Assert.Equal("t", this.viewModel.Tags.Single().Name);
        }

        [Fact]
        public void KosmographViewModel_writes_new_tag_to_persistence()
        {
            // ARRANGE

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(Enumerable.Empty<Tag>());

            var editTag = this.viewModel.CreateNewTag();

            this.tagRepository
                .Setup(r => r.Upsert(editTag.Model))
                .Returns(editTag.Model);

            editTag.Name = "tag";
            editTag.Commit();

            // ACT

            this.viewModel.Tags.Add(editTag);
        }

        [Fact]
        public void KosmographViewModel_writes_modified_tag_to_persistence()
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

            // ACT

            editTag.Name = "changed";
            editTag.Commit();
        }
    }
}