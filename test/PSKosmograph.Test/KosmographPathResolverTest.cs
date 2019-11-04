using Moq;
using PSKosmograph.PathNodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographPathResolverTest : KosmographCmdletProviderTestBase
    {
        private readonly KosmographPathResolver pathResolver;
        private readonly Mock<IKosmographProviderContext> providerContextMock;

        public KosmographPathResolverTest()
        {
            this.pathResolver = new KosmographPathResolver();
            this.providerContextMock = this.Mocks.Create<IKosmographProviderContext>();
            this.providerContextMock
                 .Setup(c => c.Drive)
                 .Returns((PSDriveInfo?)null);
        }

        [Theory]
        [InlineData("Tags", typeof(TagsNode))]
        [InlineData("Entities", typeof(EntitiesNode))]
        [InlineData("Relationships", typeof(RelationshipsNode))]
        public void KosmographPathResolver_resolves_main_nodes(string name, Type type)
        {
            // ACT

            var result = this.pathResolver.ResolvePath(this.providerContextMock.Object, name).ToArray();

            // ASSERT

            Assert.Equal(name, result.Single().Name);
            Assert.IsType(type, result.Single());
        }

        [Theory]
        [InlineData("Tags\\t", "t", typeof(TagNode))]
        [InlineData("Tags\\t\\p", "p", typeof(FacetPropertyNode))]
        public void KosmographPathResolver_resolves_path_in_Tags(string path, string name, Type type)
        {
            // ARRANGE

            this.providerContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag(WithDefaultProperty));

            // ACT

            var result = this.pathResolver.ResolvePath(this.providerContextMock.Object, path).ToArray();

            // ASSERT

            Assert.Equal(name, result.Single().Name);
            Assert.IsType(type, result.Single());
        }
    }
}