using Elementary.Compare;
using Kosmograph.Model;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class CategoryRepositoryTest
    {
        private readonly LiteRepository liteDb;
        private readonly CategoryRepository repository;
        private readonly LiteCollection<BsonDocument> categories;

        public CategoryRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.repository = new CategoryRepository(this.liteDb);
            this.categories = this.liteDb.Database.GetCollection(CategoryRepository.CollectionName);
        }

        [Fact]
        public void CategoryRepository_provides_persistent_root()
        {
            // ACT

            var result = this.repository.Root();

            // ASSERT

            Assert.NotNull(result);
            Assert.Empty(result.Name);

            var readRoot = this.categories.FindById(result.Id);

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

        [Fact]
        public void CategoryRepository_writes_category_to_collection()
        {
            // ARRANGE

            var category = new Category("category", Facet.Empty);

            // just to add a parent
            this.repository.Root().AddSubCategory(category);

            // ACT

            this.repository.Upsert(category);

            // ASSERT

            var readTag = this.categories.FindById(category.Id);

            Assert.NotNull(readTag);
            Assert.Equal(category.Id, readTag["_id"].AsGuid);
            Assert.False(readTag.ContainsKey("Parent"));
        }

        [Fact]
        public void CategoryRepository_writing_fails_on_orphaned_category()
        {
            // ARRANGE

            var category = new Category("category", Facet.Empty);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.repository.Upsert(category));

            // ASSERT

            Assert.Equal("Category must have parent.", result.Message);
            Assert.Null(this.categories.FindById(category.Id));
        }

        [Fact]
        public void CategoryRepository_writes_category_to_collection_with_sub_category_references()
        {
            // ARRANGE

            var category = this.repository.Root();
            category.AddSubCategory(new Category("cat1", Facet.Empty));

            // ACT

            this.repository.Upsert(category);

            // ASSERT

            var readCategory = this.categories.FindById(category.Id);

            Assert.NotNull(readCategory);
            Assert.Equal(category.Id, readCategory["_id"].AsGuid);
            Assert.Equal(category.SubCategories.Single().Id, readCategory["SubCategories"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(CategoryRepository.CollectionName, readCategory["SubCategories"].AsArray[0].AsDocument["$ref"].AsString);
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
            category.AddSubCategory(new Category("cat1", Facet.Empty));

            // ACT

            this.repository.Upsert(category);
            this.repository.Upsert(category.SubCategories.Single());
            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            // types are same and nothing was left out
            Assert.True(comp.AreEqual);
        }
    }
}