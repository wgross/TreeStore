using Elementary.Compare;
using Kosmograph.Model;
using LiteDB;
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
            this.categories = this.liteDb.Database.GetCollection("categories");
        }

        [Fact]
        public void Category_is_written_to_repository()
        {
            // ARRANGE

            var tag = new Category("category", Facet.Empty);

            // ACT

            this.repository.Upsert(tag);

            // ASSERT

            var readTag = this.categories.FindById(tag.Id);

            Assert.NotNull(readTag);
            Assert.Equal(tag.Id, readTag.AsDocument["_id"].AsGuid);
        }

        [Fact]
        public void Category_with_Facet_is_created_and_read_from_repository()
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
        public void Category_with_Facet_is_updated_and_read_from_repository()
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