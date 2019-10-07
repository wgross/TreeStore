using Kosmograph.Model;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographItemPropertyCmdletProviderTest : KosmographCmdletProviderTestBase
    {
        #region Get-ItemProperty /Tags/<name>

        [Fact]
        public void Powershell_retrieves_Tag_properties()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            // result contains all properties

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void Powershell_retrieves_Tag_Csharp_property_only()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t")
                    .AddParameter("Name", "Id");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            // result contains all properties

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.False(result.Single().PropertyContains(nameof(Tag.Name)));
        }

        [Fact]
        public void Powershell_retrieving_Tag_properties_returns_null_for_unknown_property()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t")
                    .AddParameter("Name", "unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            // result contains all properties

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Tags/<name>

        #region Get-ItemProperty /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_retrieves_FacetProperty_properties()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Facet.Properties.Single().Name, result[0].Property<string>("Name"));
            Assert.Equal(tag.Facet.Properties.Single().Type, result[0].Property<FacetPropertyTypeValues>("ValueType"));
        }

        [Fact]
        public void Powershell_retrieves_FacetProperty_Csharp_property_only()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p")
                    .AddParameter("Name", "Id");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>("Id"));
            Assert.False(result.Single().PropertyContains(nameof(FacetProperty.Name)));
            Assert.False(result.Single().PropertyContains(nameof(FacetProperty.Type)));
        }

        [Fact]
        public void Powershell_retrieving_FacetProperty_property_returns_null_for_unknown_property()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p")
                    .AddParameter("Name", "unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Tags/<name>/<property-name>

        #region Get-ItemProperty /Entities/<name>

        [Fact]
        public void Powershell_retrieves_Entity_properties()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(entity.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(entity.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void Powershell_retrieves_Entity_Csharp_property_only()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e")
                   .AddParameter("Name", "Name");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.False(result[0].PropertyContains("Id"));
            Assert.Equal(entity.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void Powershell_retrieving_Entity_property_returns_null_fort_unkown_property()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e")
                   .AddParameter("Name", "unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Entities/<name>

        #region Get-ItemProperty /Entities/<name>/<tag-name>

        [Fact]
        public void Powershell_retrieves_AssignedTag_properties()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
            Assert.Equal(entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2, result[0].Property<int>(tag.Facet.Properties.Single().Name));
        }

        [Fact]
        public void Powershell_retrieves_single_AssignedTag_property_by_name()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t")
                   .AddParameter("Name", "p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2, result[0].Property<int>(tag.Facet.Properties.Single().Name));
            Assert.False(result[0].PropertyContains("id"));
        }

        [Fact]
        public void Powershell_retrieving_AssignedTag_property_returns_null_for_unknown_property()
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
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t")
                   .AddParameter("Name", "unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Entities/<name>/<tag-name>

        #region Set-ItemProperty /Tags/<name>

        [Fact]
        public void Powershell_sets_Tag_name()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Set-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t")
                    .AddParameter("Name", "Name")
                    .AddParameter("Value", "changed");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            // result contains all properties

            Assert.Equal("changed", tag.Name);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Set-ItemProperty /Tags/<name>

        #region Set-ItemProperty /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_sets_FacetProperty_name()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Set-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p")
                    .AddParameter("Name", "Name")
                    .AddParameter("Value", "changed");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal("changed", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void Powershell_sets_FacetProperty_type()
        {
            // ARRANGE
            // provide the top level containers

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddStatement()
                    .AddCommand("Set-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p")
                    .AddParameter("Name", "ValueType")
                    .AddParameter("Value", FacetPropertyTypeValues.Bool);

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal(FacetPropertyTypeValues.Bool, tag.Facet.Properties.Single().Type);
        }

        #endregion Set-ItemProperty /Tags/<name>/<property-name>

        #region Set-ItemProperty /Entities/<name>

        [Fact]
        public void Powershell_sets_Entity_name()
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

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Set-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e")
                   .AddParameter("Name", "Name")
                   .AddParameter("Value", "changed");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            Assert.Equal("changed", entity.Name);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Set-ItemProperty /Entities/<name>

        #region Set-ItemProperty /Entities/<name>/<tag-name>

        [Fact]
        public void Powershell_sets_AssignedTag_property()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            var entity = DefaultEntity();
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Set-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t")
                   .AddParameter("Name", "p")
                   .AddParameter("Value", 2);

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal(2, entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Set-ItemProperty /Entities/<name>/<tag-name>
    }
}