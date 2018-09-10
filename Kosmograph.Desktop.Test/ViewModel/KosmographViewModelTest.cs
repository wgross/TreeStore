using Kosmograph.Desktop.ViewModel;
using Kosmograph.LiteDb;
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
        public void KosmographViewModel_returns_observable_tags()
        {
            // ARRANGE

            var model = new KosmographViewModel(new KosmographModel(new KosmographLiteDbPersistence()));

            // ACT

            var result = model.Tags;

            // ASSERT

            Assert.NotNull(result);
        }

        [Fact]
        public void KosmographViewModel_add_writes_tag_to_persistence()
        {
            // ARRANGE

            var tag = new Tag();

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(Enumerable.Empty<Tag>());

            this.tagRepository
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            this.viewModel.Tags.Add(new EditTagViewModel(tag));
        }
    }
}