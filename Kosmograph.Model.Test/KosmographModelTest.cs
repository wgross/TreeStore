using Moq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class KosmographModelTest
    {
        private readonly MockRepository mocks;
        private readonly Mock<IKosmographPersistence> persistence;
        private readonly Mock<ICategoryRepository> categoryRepository;
        private readonly KosmographModel model;

        public KosmographModelTest()
        {
            this.mocks = new MockRepository(MockBehavior.Strict);
            this.persistence = this.mocks.Create<IKosmographPersistence>();
            this.categoryRepository = this.mocks.Create<ICategoryRepository>();
            this.model = new KosmographModel(this.persistence.Object);
        }

        [Fact]
        public void Model_provides_root_Category()
        {
            // ARRANGE

            this.persistence
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
    }
}