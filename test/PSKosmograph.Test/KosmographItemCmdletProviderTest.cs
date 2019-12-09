using Kosmograph.Model;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographItemCmdletProviderTest : KosmographCmdletProviderTestBase
    {
        #region Get-Item Tags/Entities/Relationships

        [Fact]
        public void Powershell_retrieves_root_container()
        {
            // ACT
            // get the provider root path

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", $@"kg:\");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal($@"PSKosmograph\Kosmograph::kg:\", result.Property<string>("PSPath"));
            Assert.Equal("", result.Property<string>("PSParentPath"));
            Assert.Equal(@"kg:\", result.Property<string>("PSChildName"));
            Assert.Equal("kg", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("Kosmograph", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result.Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.True(result.Property<bool>("PSIsContainer"));
        }

        [Theory]
        [InlineData("Tags")]
        [InlineData("Entities")]
        [InlineData("Relationships")]
        public void Powershell_retrieves_single_top_level_container(string containerName)
        {
            // ACT
            // get the top level containers

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", $@"kg:\{containerName}");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal("Kosmograph", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result.Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal($@"PSKosmograph\Kosmograph::kg:\{containerName}", result.Property<string>("PSPath"));
            Assert.True(result.Property<bool>("PSIsContainer"));
            Assert.Equal(containerName, result.Property<string>("PSChildName"));
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:", result.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_retrieving_unknown_top_level_container_by_name_fails()
        {
            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", $@"kg:\unknown");

            var result = this.PowerShell.Invoke();

            // ASSERT
            // no result was returned

            Assert.True(this.PowerShell.HadErrors);
        }

        #endregion Get-Item Tags/Entities/Relationships

        #region Get-Item Tags/<name>

        [Fact]
        public void Powershell_retrieves_single_Tag_by_name()
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
            // get tag by name

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", @"kg:\Tags\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Tags\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void Powershell_retrieving_unknown_tag_by_name_fails()
        {
            // ARRANGE
            // model is missing the tag t

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns((Tag?)null);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", @"kg:\Tags\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
        }

        #endregion Get-Item Tags/<name>

        #region Get-Item Tags/<name>, Tags/<name>/<property>

        [Fact]
        public void Powershell_retrieves_single_FacetProperty_by_name()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT
            // get single property from tag t

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", @"kg:\Tags\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Facet.Properties.Single().Name, result[0].Property<string>("Name"));
            Assert.Equal(tag.Facet.Properties.Single().Type, result[0].Property<FacetPropertyTypeValues>("ValueType"));
        }

        [Fact]
        public void Powershell_retrieving_unknown_facet_property_by_name_fails()
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
            // fetch unknown property at existing tag

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-Item")
                    .AddParameter("Path", @"kg:\Tags\t\unkown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
        }

        #endregion Get-Item Tags/<name>, Tags/<name>/<property>

        #region Get-Item /Entities/<name>, /Entities/<name>/<tag-name>, /Entiites/<name>/<tag-name>/<property-name>

        [Fact]
        public void Powershell_retrieves_single_Entity_by_name()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity();
            var tag = entity.Tags.Single();

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-Item")
                   .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(entity.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(entity.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void Powershell_retrieving_unknown_entity_by_name_fails()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity();
            var tag = entity.Tags.Single();

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("unknown"))
                .Returns((Entity?)null);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-Item")
                   .AddParameter("Path", @"kg:\Entities\unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
        }

        [Fact]
        public void Powershell_retrieves_single_AssignedTag_by_name()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity();
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-Item")
                   .AddParameter("Path", @"kg:\Entities\e\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
            Assert.Equal(entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2, result[0].Property<int>(tag.Facet.Properties.Single().Name));
        }

        [Fact]
        public void Powershell_retrieves_unknown_AssignedTag_name_as_null()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-Item")
                   .AddParameter("Path", @"kg:\Entities\e\unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
        }

        [Fact]
        public void PowerShell_retrieves_single_AssignedFacetProperty_by_name()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-Item")
                   .AddParameter("Path", @"kg:\Entities\e\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Facet.Properties.Single().Name, result[0].Property<string>("Name"));
            Assert.Equal(1, result[0].Property<int>("Value"));
        }

        #endregion Get-Item /Entities/<name>, /Entities/<name>/<tag-name>, /Entiites/<name>/<tag-name>/<property-name>
    }
}