using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using Xunit;

namespace TreeStore.PsModule.Test
{
    public class KosmographItemPropertyCmdletProviderTest : TreeStoreCmdletProviderTestBase
    {
        #region Get-ItemProperty /Tags/<name>

        [Fact]
        public void PowerShell_retrieves_Tag_properties()
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void PowerShell_retrieves_Tag_Csharp_property_only()
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Id, result[0].Property<Guid>("Id"));
            Assert.False(result.Single().PropertyContains(nameof(Tag.Name)));
        }

        [Fact]
        public void PowerShell_retrieving_Tag_properties_returns_null_for_unknown_property()
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Tags/<name>

        #region Get-ItemProperty /Tags/<name>/<property-name>

        [Fact]
        public void PowerShell_retrieves_FacetProperty_properties()
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

            this.PowerShell
                .AddStatement()
                    .AddCommand("Get-ItemProperty")
                    .AddParameter("Path", @"kg:\Tags\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>("Id"));
            Assert.Equal(tag.Facet.Properties.Single().Name, result[0].Property<string>("Name"));
            Assert.Equal(tag.Facet.Properties.Single().Type, result[0].Property<FacetPropertyTypeValues>("ValueType"));
        }

        [Fact]
        public void PowerShell_retrieves_FacetProperty_single_property()
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Id, result[0].Property<Guid>(nameof(FacetProperty.Id)));
            Assert.False(result.Single().PropertyContains(nameof(FacetProperty.Name)));
            Assert.False(result.Single().PropertyContains(nameof(FacetProperty.Type)));
        }

        [Fact]
        public void PowerShell_retrieving_FacetProperty_property_returns_null_for_unknown_property()
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Tags/<name>/<property-name>

        #region Get-ItemProperty /Entities/<name>

        [Fact]
        public void PowerShell_retrieves_Entity_properties()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
               .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
               .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(entity.Id, result[0].Property<Guid>("Id"));
            Assert.Equal(entity.Name, result[0].Property<string>("Name"));
            Assert.Null(result[0].Property<long?>("t.p"));
        }

        [Fact]
        public void PowerShell_retrieves_Entity_Csharp_property_only()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.False(result[0].PropertyContains("Id"));
            Assert.Equal(entity.Name, result[0].Property<string>("Name"));
        }

        [Fact]
        public void PowerShell_retrieving_Entity_property_returns_null_fort_unkown_property()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Entities/<name>

        #region Get-ItemProperty /Entities/<name>/<tag-name>

        [Fact]
        public void PowerShell_retrieves_AssignedTag_properties()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Name, result[0].Property<string>("Name"));
            Assert.Equal(entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2, result[0].Property<string>(tag.Facet.Properties.Single().Name));
        }

        [Fact]
        public void PowerShell_retrieves_single_AssignedTag_property_by_name()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2, result[0].Property<string>(tag.Facet.Properties.Single().Name));
            Assert.False(result[0].PropertyContains("id"));
            Assert.False(result[0].PropertyContains("name"));
        }

        [Fact]
        public void PowerShell_retrieves_single_AssignedTag_property_by_unknown_name_returns_null()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                 .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
        }

        #endregion Get-ItemProperty /Entities/<name>/<tag-name>

        #region Get-ItemProperty /Entities/<name>/<tag-name>/<property-name>

        [Fact]
        public void PowerShell_retrieves_AssignedFacetProperty_properties()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT
            // from facet property name and value are retrieved, Id is ignored

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal(tag.Facet.Properties.Single().Name, result[0].Property<string>("Name"));
            Assert.Equal(tag.Facet.Properties.Single().Type, result[0].Property<FacetPropertyTypeValues>("ValueType"));
            Assert.Equal("1", result[0].Property<string>("Value"));
        }

        [Fact]
        public void PowerShell_retrieves_single_AssignedFacetProperty_property_by_name()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t\p")
                   .AddParameter("Name", "Value");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.Equal("1", result[0].Property<string>("Value"));
            Assert.False(result[0].PropertyContains("name"));
            Assert.False(result[0].PropertyContains("id"));
            Assert.False(result[0].PropertyContains("valueType"));
        }

        [Fact]
        public void PowerShell_retrieving_single_AssignedFacetProperty_property_by_unknown_name_returns_null()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Get-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t\p")
                   .AddParameter("Name", "unknown");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("TreeStore", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"TreeStore\TreeStore::kg:\Entities\e\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.False(result[0].PropertyContains("Value"));
            Assert.False(result[0].PropertyContains("name"));
            Assert.False(result[0].PropertyContains("id"));
            Assert.False(result[0].PropertyContains("valueType"));
        }

        #endregion Get-ItemProperty /Entities/<name>/<tag-name>/<property-name>

        #region Set-ItemProperty /Entities/<name>/<tag-name> -Name <property-name>, Set-ItemProperty /Entities/<name> -Name <tag-name>.<property-name>

        [Fact]
        public void PowerShell_sets_AssignedFacetProperty_at_entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Set-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e")
                   .AddParameter("Name", "t.p")
                   .AddParameter("Value", "2");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal("2", entity.TryGetFacetProperty(tag.Facet.Properties.Single()).value);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void PowerShell_sets_AssignedFacetProperty_at_assigned_tag()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddStatement()
                   .AddCommand("Set-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t")
                   .AddParameter("Name", "p")
                   .AddParameter("Value", "2");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal("2", entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void PowerShell_sets_AssignedFacetProperty_at_property()
        {
            // ARRANGE
            // provide a tag and an entity using this tag

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Set-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t\p")
                   .AddParameter("Name", "Value")
                   .AddParameter("Value", "2");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.Equal("2", entity.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Set-ItemProperty /Entities/<name>/<tag-name> -Name <property-name>, Set-ItemProperty /Entities/<name> -Name <tag-name>.<property-name>

        #region Clear-ItemProperty /Entities/e/<name>/<tag-name> -Name <property-name>

        [Fact]
        public void PowerShell_clears_AssignedFacetProperty_at_entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Clear-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e")
                   .AddParameter("Name", "t.p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(entity.Values);
        }

        [Fact]
        public void PowerShell_clears_AssignedFacetProperty_at_Assinged_tag()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            var tag = entity.Tags.Single();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), "1");
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT

            this.PowerShell
               .AddStatement()
                   .AddCommand("Clear-ItemProperty")
                   .AddParameter("Path", @"kg:\Entities\e\t")
                   .AddParameter("Name", "p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(entity.Values);
        }

        //todo: clear assigned tag property

        #endregion Clear-ItemProperty /Entities/e/<name>/<tag-name> -Name <property-name>
    }
}