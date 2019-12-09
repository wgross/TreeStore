using Kosmograph.Model;
using Moq;
using PSKosmograph.PathNodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographContainerCmdletProviderTest : KosmographCmdletProviderTestBase
    {
        #region New-Item /Tags/<name>

        [Fact]
        public void Powershell_creates_Tag_by_path()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("tag"))
                .Returns((Tag?)null);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => t.Name.Equals("tag"))))
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Tags\tag");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Tags\tag", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("tag", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Tag, result[0].Property<KosmographItemType>("ItemType"));
        }

        [Fact]
        public void Powershell_creates_Tag_by_name()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("tag"))
                .Returns((Tag?)null);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => t.Name.Equals("tag"))))
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Tags")
                .AddParameter("Name", "tag");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Tags\tag", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("tag", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Tag, result[0].Property<KosmographItemType>("ItemType"));
        }

        #endregion New-Item /Tags/<name>

        #region New-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_creates_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

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
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Tags\t\p")
                .AddParameter("ValueType", FacetPropertyTypeValues.Bool);

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("p", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.FacetProperty, result[0].Property<KosmographItemType>("ItemType"));
        }

        #endregion New-Item /Tags/<name>/<property-name>

        #region New-Item /Entities/<name>

        [Fact]
        public void Powershell_creates_Entity_by_path()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("e", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Entity, result[0].Property<KosmographItemType>("ItemType"));
        }

        [Fact]
        public void Powershell_creates_entity_by_name()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities")
                    .AddParameter("Name", @"e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("e", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Entity, result[0].Property<KosmographItemType>("ItemType"));
        }

        [Fact]
        public void Powershell_creating_duplicate_entity_fails()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
        }

        [Fact]
        public void Powershell_creates_Entity_with_Tag_attached()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns((Entity?)null);

            Entity? entity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Callback<Entity>(e => entity = e)
                .Returns<Entity>(e => e);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag());

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\e")
                    .AddParameter("Tags", "t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("e", result[0].Property<string>("Name"));

            Assert.Equal("t", entity!.Tags.Single().Name);
        }

        #endregion New-Item /Entities/<name>

        #region New-Item /Entities/<name>/<tag-name>

        [Fact]
        public void Powershell_assigns_Tag_to_Entiity()
        {
            // ARRANGE

            var entity = DefaultEntity(WithoutTag);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag());

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Entities\e\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\e\t", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("t", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.AssignedTag, result[0].Property<KosmographItemType>("ItemType"));
        }

        #endregion New-Item /Entities/<name>/<tag-name>

        #region Get-ChildItem /Tags/<name>

        [Fact]
        public void PowerShell_reads_facet_property_as_tag_child()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"kg:\Tags\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal("p", result.Single().Property<string>("Name"));
            Assert.Equal(FacetPropertyTypeValues.String, result.Single().Property<FacetPropertyTypeValues>("ValueType"));
        }

        #endregion Get-ChildItem /Tags/<name>

        #region Get-ChildItem /Entities/<name>

        [Fact]
        public void PowerShell_reads_assigned_tags_as_entity_children()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("Get-ChildItem")
                    .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal("t", result.Single().Property<string>("Name"));
            Assert.Equal(entity.Tags.Single().Id, result.Single().Property<Guid>("Id"));
        }

        #endregion Get-ChildItem /Entities/<name>

        #region Get-ChildItem /Entities/<name>/<tag-name>

        [Fact]
        public void PowerShell_reads_assigned_properties_as_assigned_tag_children()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"kg:\Entities\e\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.Equal("p", result.Single().Property<string>("Name"));
        }

        #endregion Get-ChildItem /Entities/<name>/<tag-name>

        #region Copy-Item /Tags/<name>

        [Fact]
        public void Powershell_copies_Tag()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("tt"))
                .Returns((Tag?)null);

            Tag? createdTag = null;
            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Callback<Tag>(t => createdTag = t)
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
                .AddCommand("Copy-Item")
                .AddParameter("Path", $@"kg:\Tags\t")
                .AddParameter("Destination", $@"kg:\Tags\tt");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Copy-Item /Tags/<name>

        #region Copy-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_copies_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var tag2 = DefaultTag(WithoutProperty, t => t.Name = "tt");

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("tt"))
                .Returns(tag2);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag2))
                .Returns(tag2);

            // ACT

            this.PowerShell
                .AddCommand("Copy-Item")
                .AddParameter("Path", $@"kg:\Tags\t\p")
                .AddParameter("Destination", $@"kg:\Tags\tt");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Copy-Item /Tags/<name>/<property-name>

        #region Copy-Item /Entities/<name>

        [Fact]
        public void Powershell_copies_Entity()
        {
            // ARRANGE

            var tag = DefaultEntity();

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(tag);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("ee"))
                .Returns((Entity?)null);

            Entity? createdEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(t => createdEntity = t)
                .Returns<Entity>(t => t);

            // ACT

            this.PowerShell
                .AddCommand("Copy-Item")
                .AddParameter("Path", $@"kg:\Entities\e")
                .AddParameter("Destination", $@"kg:\Entities\ee");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Copy-Item /Entities/<name>

        #region Rename-Item /Tags/<name>

        [Fact]
        public void Powershell_renames_Tag()
        {
            // ARRANGE

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => "tt".Equals(t.Name))))
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
               .AddCommand("Rename-Item")
               .AddParameter("Path", $@"kg:\Tags\t")
               .AddParameter("NewName", "tt");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Rename-Item /Tags/<name>

        #region Rename-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_renames_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => "pp".Equals(t.Facet.Properties.Single().Name))))
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
               .AddCommand("Rename-Item")
               .AddParameter("Path", $@"kg:\Tags\t\p")
               .AddParameter("NewName", "pp");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Rename-Item /Tags/<name>/<property-name>

        #region Rename-Item /Entities/<name>

        [Fact]
        public void Powershell_renames_Entity()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => "ee".Equals(e.Name))))
                .Returns<Entity>(e => e);

            // ACT

            this.PowerShell
               .AddCommand("Rename-Item")
               .AddParameter("Path", $@"kg:\Entities\e")
               .AddParameter("NewName", "ee");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Rename-Item /Entities/<name>

        #region Remove-Item /Tags/<name>

        [Fact]
        public void Powershell_removes_empty_tag()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Tags\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void Powershell_removes_tag_with_properties()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Tags\t")
                .AddParameter("Recurse");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Remove-Item /Tags/<name>

        #region Remove-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_removes_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

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
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Tags\t\p");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Remove-Item /Tags/<name>/<property-name>

        #region Remove-Item /Entities/<name>

        [Fact]
        public void Powershell_removes_empty_entity()
        {
            // ARRANGE

            var entity = DefaultEntity(WithoutTag);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void Powershell_removes_entity_with_tags()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Entities\e")
                .AddParameter("Recurse");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Remove-Item /Entities/<name>

        #region Remove-Item /Entities/<name>/<tag-name>

        [Fact]
        public void Powershell_removes_assigned_tag()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

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
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Entities\e\t")
                .AddParameter("Recurse");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
            Assert.Empty(entity.Tags);
        }

        #endregion Remove-Item /Entities/<name>/<tag-name>
    }
}