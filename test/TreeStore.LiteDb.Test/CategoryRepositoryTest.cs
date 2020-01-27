using Elementary.Compare;
using TreeStore.Model;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;
using static TreeStore.LiteDb.Test.TestDataSources;

namespace TreeStore.LiteDb.Test
{
    public class CategoryRepositoryTest
    {
        private readonly LiteRepository liteDb;
        private readonly CategoryRepository repository;
        private readonly LiteCollection<BsonDocument> categoriesCollection;

        public CategoryRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.repository = new CategoryRepository(this.liteDb);
            this.categoriesCollection = this.liteDb.Database.GetCollection("categories");
        }

        #region Root

        [Fact]
        public void CategoryRepository_provides_persistent_root()
        {
            // ACT

            var result = this.repository.Root();

            // ASSERT

            Assert.NotNull(result);
            Assert.Empty(result.Name);
            Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), result.Id);

            var readRoot = this.categoriesCollection.FindById(result.Id);

            Assert.NotNull(readRoot);
        }

        [Fact]
        public void CategoryRepository_provides_persistent_root_with_same_transient_instance()
        {
            var root = this.repository.Root();

            // ACT

            var result1 = this.repository.Root();
            var result2 = this.repository.FindById(root.Id);

            // ASSERT

            Assert.Same(root, result1);
            Assert.Same(root, result2);
        }

        #endregion Root

        #region Create

        [Fact]
        public void CategoryRepository_creates_subcategory_to_root()
        {
            // ARRANGE

            var category = new Category("category");

            // just to add a parent
            this.repository.Root().AddSubCategory(category);

            // ACT

            this.repository.Upsert(category);

            // ASSERT

            // category was created
            var categoryInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(categoryInDb);
            Assert.Equal(category.Id, categoryInDb["_id"].AsGuid);
            Assert.Equal(category.Name, categoryInDb["Name"].AsString);
            Assert.False(categoryInDb.ContainsKey("Parent"));

            // root has been updated
            var rootInDb = this.categoriesCollection.FindById(this.repository.Root().Id);

            Assert.Equal(category.Id, rootInDb["SubCategories"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(this.repository.CollectionName, rootInDb["SubCategories"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void CategoryRepository_creating_fails_for_orphaned_category()
        {
            // ARRANGE

            var category = new Category("category");

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.repository.Upsert(category));

            // ASSERT

            Assert.Equal("Category must have parent.", result.Message);
            Assert.Null(this.categoriesCollection.FindById(category.Id));
        }

        [Fact]
        public void CategoryRepository_writes_and_reads_category_with_Facet()
        {
            // ARRANGE

            var category = this.repository.Root();
            category.AssignFacet(new Facet("facet", new FacetProperty("prop")));

            // ACT

            this.repository.Upsert(category);
            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            Assert.True(comp.AreEqual);
        }

        [Fact]
        public void CategoryRepository_writes_and_reads_category_with_subcategory()
        {
            // ARRANGE

            var category = this.repository.Root();
            category.AddSubCategory(new Category("cat1"));

            // ACT

            this.repository.Upsert(category);
            this.repository.Upsert(category.SubCategories.Single());
            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            // types are same and nothing was left out
            Assert.True(comp.AreEqual);
        }

        #endregion Create

        #region Read

        [Fact]
        public void CategoryRepository_reads_category_by_id()
        {
            // ARRANGE

            var category = new Category("category");

            // just to add a parent
            this.repository.Root().AddSubCategory(category);
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindById(category.Id);

            // ASSERT

            Assert.Same(category, result);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_id_with_subcategory()
        {
            // ARRANGE

            var category = this.repository.Root();
            category.AddSubCategory(new Category("cat1"));
            this.repository.Upsert(category);
            this.repository.Upsert(category.SubCategories.Single());

            // ACT

            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            // types are same and nothing was left out
            Assert.True(comp.AreEqual);
        }

        [Fact]
        public void CategoryRepository_reading_category_by_id_returns_null_on_missing_category()
        {
            // ACT

            var result = this.repository.FindById(Guid.NewGuid());

            // ASSERT

            Assert.Null(result);
        }

        [Theory]
        [InlineData("c")]
        [InlineData("C")]
        public void CategoryRepository_reads_category_by_category_and_name(string name)
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByCategoryAndName(this.repository.Root(), name);

            // ASSERT

            Assert.Equal(category.Id, result.Id);
        }

        [Fact]
        public void CategeoryRepository_reading_category_by_category_and_name_returns_null_on_unkown_id()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByCategoryAndName(new Category(), "name");

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void CategeoryRepository_reads_child_categoriesby_id()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByCategory(this.repository.Root());

            // ASSERT

            Assert.Equal(category.Id, result.Single().Id);
        }

        [Fact]
        public void CategoryRespository_reading_child_categories_by_id_is_empty_on_unknown_id()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByCategory(new Category());

            // ASSERT

            Assert.Empty(result);
        }

        #endregion Read

        #region //Delete

        //[Fact]
        //public void CategoryRepository_deletes_empty_category()
        //{
        //    // ARRANGE

        //    var category = DefaultCategory(this.repository.Root());
        //    this.repository.Upsert(category);

        //    // ACT

        //    var result = this.repository.Delete(category);

        //    // ASSERT

        //    Assert.True(result);

        //    // category was created
        //    var categoryInDb = this.categoriesCollection.FindById(category.Id);

        //    Assert.Null(categoryInDb);

        //    // root has been updated
        //    var rootInDb = this.categoriesCollection.FindById(this.repository.Root().Id);

        //    Assert.Empty(rootInDb["SubCategories"].AsArray);
        //}

        //[Fact]
        //public void CategoryRepository_rejects_deleting_nonempty_category()
        //{
        //    // ARRANGE

        //    var category = this.repository.Upsert(DefaultCategory(this.repository.Root()));
        //    this.repository.Upsert(DefaultCategory(category));

        //    // ACT

        //    var result = this.repository.Delete(category);

        //    // ASSERT

        //    Assert.False(result);

        //    // category stil exists
        //    Assert.NotNull(this.categoriesCollection.FindById(category.Id));
        //}

        //[Fact]
        //public void CategoryRepository_deletes_category_recursively()
        //{
        //    // ARRANGE

        //    var category = this.repository.Upsert(DefaultCategory(this.repository.Root()));
        //    var subCategory = this.repository.Upsert(DefaultCategory(category));

        //    // ACT

        //    var result = this.repository.Delete(category, recurse: true); ;

        //    // ASSERT

        //    Assert.True(result);

        //    // category and subcategeory are gone
        //    Assert.Null(this.categoriesCollection.FindById(category.Id));
        //    Assert.Null(this.categoriesCollection.FindById(subCategory.Id));

        //    // root has been updated in DB
        //    var rootInDb = this.categoriesCollection.FindById(this.repository.Root().Id);

        //    Assert.Empty(rootInDb["SubCategories"].AsArray);
        //}

        #endregion //Delete
    }
}