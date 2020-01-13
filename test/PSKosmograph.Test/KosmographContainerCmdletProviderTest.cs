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
        public void PowerShell_creates_Tag_by_path()
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
        public void PowerShell_creates_Tag_by_name()
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
        public void PowerShell_creates_facet_property()
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

        #region New-Item /Entities/<name> -ItemType Entity

        [Fact]
        public void PowerShell_creates_entity_by_path()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

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
        public void PowerShell_creates_entity_by_name()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

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
        public void PowerShell_creates_Entity_with_Tag_attached()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Entity?)null);

            Entity? entity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Callback<Entity>(e => entity = e)
                .Returns<Entity>(e => e);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

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

        #endregion New-Item /Entities/<name> -ItemType Entity

        #region New-Item /Entities/<entity-name>/<tag-name>

        [Fact]
        public void PowerShell_assigns_Tag_to_Entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithoutTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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

        #endregion New-Item /Entities/<entity-name>/<tag-name>

        #region New-Item /Entities/<name> -ItemType Category

        [Fact]
        public void PowerShell_creates_entity_category_by_path()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.Is<Category>(e => e.Name.Equals("c"))))
                .Returns<Category>(c => c);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns((Entity?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns((Category?)null);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\c")
                    .AddParameter("ItemType", "Category");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\c", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("c", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Category, result[0].Property<KosmographItemType>("ItemType"));
        }

        #endregion New-Item /Entities/<name> -ItemType Category

        #region New-Item /Entities/<category-name>/<name> -ItemType Entity

        [Fact]
        public void PowerShell_creates_entity_in_category_by_path()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Entity>());

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\c\e")
                    .AddParameter("ItemType", "Entity");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::kg:\Entities\c\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("e", result[0].Property<string>("Name"));
            Assert.Equal(KosmographItemType.Entity, result[0].Property<KosmographItemType>("ItemType"));
        }

        #endregion New-Item /Entities/<category-name>/<name> -ItemType Entity

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

        #region Get-ChildItem /Entities

        [Fact]
        public void PowerShell_reads_Entities_children()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(rootCategory))
                .Returns(entity.Yield());

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategory(rootCategory))
                .Returns(subCategory.Yield());

            // ACT

            this.PowerShell
                .AddCommand("Get-ChildItem")
                    .AddParameter("Path", @"kg:\Entities");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(2, result.Length);
            Assert.Equal("c", result.First().Property<string>("Name"));
            Assert.Equal(subCategory.Id, result.First().Property<Guid>("Id"));
            Assert.Equal("e", result.Last().Property<string>("Name"));
            Assert.Equal(entity.Id, result.Last().Property<Guid>("Id"));
        }

        #endregion Get-ChildItem /Entities

        #region Get-ChildItem /Entities/<entity-name>

        [Fact]
        public void PowerShell_reads_assigned_tags_as_entity_children()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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

        #endregion Get-ChildItem /Entities/<entity-name>

        #region Get-ChildItem /Entities/<category-name>

        [Fact]
        public void PowerShell_reads_categories_children()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, subCategory.Name))
                .Returns(subCategory);

            var category = DefaultCategory(c => c.Name = "cc");
            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(category.Yield());

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(entity.Yield());

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            // ACT

            this.PowerShell
                .AddCommand("Get-ChildItem")
                    .AddParameter("Path", @"kg:\Entities\c");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(2, result.Length);
            Assert.Equal("cc", result.First().Property<string>("Name"));
            Assert.Equal(category.Id, result.First().Property<Guid>("Id"));
            Assert.Equal("e", result.Last().Property<string>("Name"));
            Assert.Equal(entity.Id, result.Last().Property<Guid>("Id"));
        }

        #endregion Get-ChildItem /Entities/<category-name>

        #region Get-ChildItem /Entities/<entity-name>/<tag-name>

        [Fact]
        public void PowerShell_reads_assigned_properties_as_assigned_tag_children()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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

        #endregion Get-ChildItem /Entities/<entity-name>/<tag-name>

        #region Copy-Item /Tags/<name>

        [Fact]
        public void PowerShell_copies_Tag()
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
        public void PowerShell_copies_facet_property()
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

        #region Copy-Item /Entities/<entity-name>

        [Fact]
        public void PowerShell_copies_Entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            var tag = DefaultEntity();

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(tag);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
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

        #endregion Copy-Item /Entities/<entity-name>

        #region Copy-Item /Entities/<entity-name> /Entities/<category-name>

        [Fact]
        public void PowerShell_copies_Entity_to_Category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("Copy-Item")
                .AddParameter("Path", $@"kg:\Entities\e")
                .AddParameter("Destination", $@"kg:\Entities\c");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Copy-Item /Entities/<entity-name> /Entities/<category-name>

        //todo: copy category to another category
        //todo: copy entity to another category

        //todo: move entity to ano,ter category
        //todo: move category to another category

        #region Rename-Item /Tags/<name>

        [Fact]
        public void PowerShell_renames_Tag()
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
        public void PowerShell_renames_facet_property()
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

        #region Rename-Item /Entities/<entity-name>

        [Fact]
        public void PowerShell_renames_Entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithEntityCategory(rootCategory));
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
                .Returns((Entity?)null);

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

        #endregion Rename-Item /Entities/<entity-name>

        #region Rename-Item /Entities/<category-name>

        [Fact]
        public void PowerShell_renames_Category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, subCategory.Name))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.Is<Category>(e => "cc".Equals(e.Name))))
                .Returns<Category>(c => c);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "cc"))
                .Returns((Entity?)null);

            // ACT

            this.PowerShell
               .AddCommand("Rename-Item")
                   .AddParameter("Path", $@"kg:\Entities\c")
                   .AddParameter("NewName", "cc");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Rename-Item /Entities/<category-name>

        #region Remove-Item /Tags/<name>

        [Fact]
        public void PowerShell_removes_empty_tag()
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
        public void PowerShell_removes_tag_with_properties()
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
        public void PowerShell_removes_facet_property()
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

        #region Remove-Item /Entities/<category-name>

        [Fact]
        public void PowerShell_removes_empty_category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, subCategory.Name))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Category>());

            this.PersistenceMock
                .Setup(r => r.DeleteCategory(subCategory, false))
                .Returns(true);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Entity>());

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Entities\c");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void PowerShell_removes_category_with_children()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, subCategory.Name))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Category>());

            this.PersistenceMock
                .Setup(r => r.DeleteCategory(subCategory, true))
                .Returns(true);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Entity>());

            // ACT

            this.PowerShell
                .AddCommand("Remove-Item")
                .AddParameter("Path", $@"kg:\Entities\c")
                .AddParameter("Recurse");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Remove-Item /Entities/<category-name>

        #region Remove-Item /Entities/<entity-name>

        [Fact]
        public void PowerShell_removes_empty_entity()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithoutTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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
        public void PowerShell_removes_entity_with_tags()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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

        #endregion Remove-Item /Entities/<entity-name>

        #region Remove-Item /Entities/<entity-name>/<tag-name>

        [Fact]
        public void PowerShell_removes_assigned_tag()
        {
            // ARRANGE

            this.ArrangeEmptyRootCategory(out var rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            var entity = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
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

        #endregion Remove-Item /Entities/<entity-name>/<tag-name>

        #region Move-Item /Entities/<entity-name> /Entities/<category-name>

        [Fact]
        public void PowerShell_moves_entity_to_category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("Move-Item")
                .AddParameter("Path", $@"kg:\Entities\e")
                .AddParameter("Destination", $@"kg:\Entities\c");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void PowerShell_moves_entity_to_category_with_new_name()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e-src"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e-dst"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var entity = DefaultEntity(WithDefaultTag);
            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "e-src"))
                .Returns(entity);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Entity>());

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "e-dst"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT

            this.PowerShell
                .AddCommand("Move-Item")
                .AddParameter("Path", $@"kg:\Entities\e-src")
                .AddParameter("Destination", $@"kg:\Entities\c\e-dst");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Move-Item /Entities/<entity-name> /Entities/<category-name>

        #region Move-Item /Entities/<category-name-2> /Entities/<category-name>

        [Fact]
        public void PowerShell_moves_category_to_category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);
            var subCategory2 = DefaultCategory(c => c.Name = "c-src");

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c-src"))
                .Returns(subCategory2);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "c-src"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "c-src"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "c-src"))
                .Returns((Entity?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(subCategory))
                .Returns(subCategory);

            // ACT

            this.PowerShell
                .AddCommand("Move-Item")
                .AddParameter("Path", $@"kg:\Entities\c-src")
                .AddParameter("Destination", $@"kg:\Entities\c");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        [Fact]
        public void PowerShell_moves_category_to_category_with_new_name()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);
            var subCategory2 = DefaultCategory(c => c.Name = "c-src");

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns(subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c-src"))
                .Returns(subCategory2);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "c-dst"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(subCategory, "c-dst"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(subCategory))
                .Returns(Enumerable.Empty<Entity>());

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(subCategory))
                .Returns(subCategory);

            // ACT

            this.PowerShell
                .AddCommand("Move-Item")
                .AddParameter("Path", $@"kg:\Entities\c-src")
                .AddParameter("Destination", $@"kg:\Entities\c\c-dst");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Move-Item /Entities/<category-name-2> /Entities/<category-name>
    }
}