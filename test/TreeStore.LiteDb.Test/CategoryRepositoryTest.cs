using LiteDB;
using System;
using System.IO;
using System.Linq;
using TreeStore.Model;
using Xunit;
using static TreeStore.LiteDb.Test.TestDataSources;

namespace TreeStore.LiteDb.Test
{
    public class CategoryRepositoryTest
    {
        private readonly LiteRepository liteDb;
        private readonly CategoryRepository repository;
        private readonly ILiteCollection<BsonDocument> categoriesCollection;

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

            // root was created
            var resultInDb = this.categoriesCollection.FindById(result.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(result.Id, resultInDb["_id"].AsGuid);
            Assert.Null(resultInDb["Name"].AsString);
            Assert.False(resultInDb.ContainsKey("Parent"));
            Assert.Equal("_<root>", resultInDb["UniqueName"].AsString);
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

            var result = this.repository.Upsert(category);

            // ASSERT

            Assert.Same(category, result);
            Assert.NotEqual(Guid.Empty, result.Id);

            // category was created
            var resultInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(category.Id, resultInDb["_id"].AsGuid);
            Assert.Equal(category.Name, resultInDb["Name"].AsString);
            Assert.Equal(this.repository.Root().Id, resultInDb["Parent"].AsDocument["$id"].AsGuid);
            Assert.Equal(category.UniqueName, resultInDb["UniqueName"].AsString);
            Assert.Equal("categories", resultInDb["Parent"].AsDocument["$ref"].AsString);
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
        public void CategoryRepository_writes_category_with_Facet()
        {
            // ARRANGE

            var category = new Category("category");

            this.repository.Root().AddSubCategory(category);
            category.AssignFacet(new Facet("facet", new FacetProperty("prop")));

            // ACT

            var result = this.repository.Upsert(category);

            // ASSERT

            Assert.Same(category, result);
            Assert.NotEqual(Guid.Empty, result.Id);

            // category was created
            var resultInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(category.Id, resultInDb["_id"].AsGuid);
            Assert.Equal(category.Name, resultInDb["Name"].AsString);
            Assert.Equal(this.repository.Root().Id, resultInDb["Parent"].AsDocument["$id"].AsGuid);
            Assert.Equal("categories", resultInDb["Parent"].AsDocument["$ref"].AsString);
            Assert.Equal(category.UniqueName, resultInDb["UniqueName"].AsString);
            Assert.True(resultInDb.ContainsKey("Facet")); // no further inspection. Feature isn't used.
        }

        #endregion Create

        #region Read

        [Fact]
        public void CategoryRepository_reads_category_by_id_including_parent()
        {
            // ARRANGE

            var category = new Category("category");

            // just to add a parent
            this.repository.Root().AddSubCategory(category);
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindById(category.Id);

            // ASSERT

            Assert.NotSame(category, result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(this.repository.Root().Id, result.Parent.Id);
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
        public void CategoryRepository_reads_category_by_parent_and_name(string name)
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByParentAndName(this.repository.Root(), name);

            // ASSERT

            Assert.Equal(category.Id, result.Id);
        }

        [Fact]
        public void CategoryRepository_reading_category_by_parent_and_name_returns_null_on_unkown_id()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByParentAndName(new Category(), "name");

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_parent()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);
            var subcategory = DefaultCategory(category, c => c.Name = "sub");
            category.AddSubCategory(subcategory);
            this.repository.Upsert(subcategory);

            // ACT

            var result = this.repository.FindByParent(this.repository.Root());

            // ASSERT

            Assert.Equal(category.Name, result.Single().Name);
            Assert.Equal(category.Id, result.Single().Id);
        }

        [Fact]
        public void CategoryRepository_reads_subcategory_by_parent()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root(), c => c.Name = "cat");
            this.repository.Upsert(category);
            var subcategory = DefaultCategory(category, c => c.Name = "sub");
            category.AddSubCategory(subcategory);
            this.repository.Upsert(subcategory);

            // ACT

            var result = this.repository.FindByParent(category);

            // ASSERT

            Assert.Equal(subcategory.Id, result.Single().Id);
        }

        [Fact]
        public void CategoryRespository_reading_child_categories_by_id_is_empty_on_unknown_id()
        {
            // ARRANGE

            var category = DefaultCategory(this.repository.Root());
            this.repository.Upsert(category);

            // ACT

            var result = this.repository.FindByParent(new Category());

            // ASSERT

            Assert.Empty(result);
        }

        #endregion Read
    }
}