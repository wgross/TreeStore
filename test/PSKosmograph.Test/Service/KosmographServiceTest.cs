using PSKosmograph.Service;
using Xunit;

namespace PSKosmograph.Test.Service
{
    public class KosmographServiceTest : KosmographCmdletProviderTestBase
    {
        private readonly KosmographProviderService service;

        public KosmographServiceTest()
        {
            this.service = new KosmographProviderService(this.Model);
        }

        [Theory]
        [InlineData("Tags")]
        [InlineData("Entities")]
        [InlineData("Relationships")]
        public void KosmographService_retrieves_Tag_container_by_path(string containerName)
        {
            // ARRANGE

            var (_, path) = KosmographPath.TryParse(containerName);

            // ACT
            // retrieve tag container by Path

            var result = this.service.GetContainerByPath(path);

            // ASSERT
            // result is KosmographContainer instance representing the Tags

            Assert.Equal(containerName, result.Name);
            Assert.True(result.IsContainer);
        }
    }
}