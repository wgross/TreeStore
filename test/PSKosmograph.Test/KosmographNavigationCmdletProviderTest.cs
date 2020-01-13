using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographNavigationCmdletProviderTest : KosmographCmdletProviderTestBase
    {
        #region Test-Path /Entities, /Tags, /Relationships

        [Theory]
        [InlineData("Tags")]
        [InlineData("Entities")]
        [InlineData("Relationships")]
        public void PowerShell_test_top_level_container(string containerName)
        {
            // ACT
            // test the top level containers

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", $@"kg:\{containerName}")
                    .AddParameter("PathType", "Container");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(result.As<bool>());
        }

        #endregion Test-Path /Entities, /Tags, /Relationships

        #region Test-Path /Tags/<name>, /Tags/<name>/<property-name>

        [Fact]
        public void PowerShell_tests_Tag_container_by_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", @"kg:\Tags\t")
                    .AddParameter("PathType", "Container");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(result.As<bool>());
        }

        [Fact]
        public void PowerShell_tests_tags_FacetProperty_leaf_by_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", @"kg:\Tags\t\p")
                    .AddParameter("PathType", "Leaf"); ;

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(result.As<bool>());
        }

        #endregion Test-Path /Tags/<name>, /Tags/<name>/<property-name>

        #region Test-Path /Entities/<name>, /Entities/<name>/<tag-name>, /Entities/<name>/<tag-name>/<property-name>

        [Fact]
        public void PowerShell_tests_Entity_container_by_name()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
              .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
              .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", @"kg:\Entities\e")
                    .AddParameter("PathType", "Container");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(result.As<bool>());
        }

        [Fact]
        public void PowerShell_tests_entities_AssignedTag_container_by_name()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
              .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
              .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", @"kg:\Entities\e\t")
                    .AddParameter("PathType", "Container");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.False(result.As<bool>());
        }

        [Fact]
        public void PowerShell_tests_entities_AssignedFacetProperty_leaf_by_name()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
              .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
              .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Test-Path")
                    .AddParameter("Path", @"kg:\Entities\e\t\p")
                    .AddParameter("PathType", "Leaf");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.False(result.As<bool>());
        }
    }

    #endregion Test-Path /Entities/<name>, /Entities/<name>/<tag-name>, /Entities/<name>/<tag-name>/<property-name>
}