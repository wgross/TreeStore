using Moq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class KosmographModelTest : ModelTestBase
    {
        private readonly MockRepository mocks;
        private readonly Mock<ICategoryRepository> categoryRepository;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly KosmographModel model;

        public KosmographModelTest()
        {
            this.mocks = new MockRepository(MockBehavior.Strict);
            this.categoryRepository = this.mocks.Create<ICategoryRepository>();
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.model = this.NewModel();
        }

        [Fact]
        public void KosmographModel_provides_root_Category()
        {
            // ARRANGE

            this.Persistence
                .Setup(p => p.Categories)
                .Returns(this.categoryRepository.Object);

            this.categoryRepository
                .Setup(p => p.Root())
                .Returns(new Category());

            // ACT

            var result = this.model.RootCategory();

            // ASSERT

            Assert.NotNull(result);
        }

        [Fact]
        public void KosmographModel_provides_tags()
        {
            // ARRANGE

            this.Persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);

            // ACT

            var result = this.model.Tags;

            // ASSERT

            Assert.Same(result, this.tagRepository.Object);
        }

        [Fact]
        public void KosmographModel_disposes_persistence_on_model_disposing()
        {
            // ARRANGE

            this.Persistence
                .Setup(p => p.Dispose());

            // ACT

            this.model.Dispose();
        }
    }
}