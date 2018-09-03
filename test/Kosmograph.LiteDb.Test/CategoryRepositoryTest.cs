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
        public void CategoryRepository_writes_category_to_collection()
        {
            // ARRANGE

            var category = new Category("category", Facet.Empty);

            // ACT

            this.repository.Upsert(category);

            // ASSERT

            var readTag = this.categories.FindById(category.Id);

            Assert.NotNull(readTag);
            Assert.Equal(category.Id, readTag["_id"].AsGuid);
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

            var category = new Category("category", Facet.Empty, new Category("cat1", Facet.Empty));

            // ACT

            this.repository.Upsert(category);

            // ASSERT

            var readTag = this.categories.FindById(category.Id);

            Assert.NotNull(readTag);
            Assert.Equal(category.Id, readTag["_id"].AsGuid);
        }

        [Fact]
        public void CategoryRepository_with_Facet_is_created_and_read_from_repository()
        {
            // ARRANGE

            var category = new Category("category", new Facet("facet", new FacetProperty("prop")));

            // ACT

            this.repository.Upsert(category);
            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            // subcategory is Enumerable.Empty after init and List after read
            Assert.Equal(nameof(Category.SubCategories), comp.Different.Types.Single());
            Assert.Equal(nameof(Category.SubCategories), comp.Different.Values.Single());
        }

        [Fact]
        public void CategoryRepository_with_Facet_is_updated_and_read_from_repository()
        {
            // ARRANGE

            var category = new Category("category", new Facet("facet", new FacetProperty("prop")));
            this.repository.Upsert(category);

            // ACT

            category.Name = "name2";
            category.AssignFacet(new Facet("facet2", new FacetProperty("prop2")));

            this.repository.Upsert(category);
            var result = this.repository.FindById(category.Id);

            // ASSERT

            var comp = category.DeepCompare(result);

            // subcategory is Enumerable.Empty after init and List after read
            Assert.Equal(nameof(Category.SubCategories), comp.Different.Types.Single());
            Assert.Equal(nameof(Category.SubCategories), comp.Different.Values.Single());
        }
    }
}