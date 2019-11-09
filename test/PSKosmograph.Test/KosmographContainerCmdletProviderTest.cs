using Kosmograph.Model;
using Moq;
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
        }

        #endregion New-Item /Tags/<name>

        #region New-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_creates_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(t => t.Facet.Properties.Clear());

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
        }

        #endregion New-Item /Tags/<name>/<property-name>

        #region New-Item /Entities/<name>

        [Fact]
        public void PowerShell_creates_Entity_by_path()
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
        }

        [Fact]
        public void PowerShell_creates_entity_by_name()
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
        }

        [Fact]
        public void PowerShell_creating_duplicate_entity_fails()
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
        public void PowerShell_creates_Entity_with_Tag_attached()
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
        }

        #endregion New-Item /Entities/<name>/<tag-name>

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
                .AddParameter("Path", $@"kg:\Entities\e\t");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
            Assert.Empty(entity.Tags);
        }

        #endregion Remove-Item /Entities/<name>/<tag-name>
    }
}