using Kosmograph.Messaging;
using LiteDB;
using System.IO;
using Xunit;
using static Kosmograph.LiteDb.Test.TestDataSources;

namespace Kosmograph.LiteDb.Test
{
    public class CategoryRemovalTraverserTest
    {
        private readonly EntityRepository entityRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly CategoryRemovalTraverser traverser;

        public CategoryRemovalTraverserTest()
        {
            var tmp = new LiteRepositoryAdapater(new LiteRepository(new MemoryStream()));
            this.entityRepository = new EntityRepository(tmp.LiteRepository, KosmographMessageBus.Default.Entities);
            this.categoryRepository = new CategoryRepository(tmp.LiteRepository);
            this.traverser = new CategoryRemovalTraverser(this.categoryRepository, this.entityRepository);
        }

        [Fact]
        public void CategoryRemovalTraverser_removes_empty_category()
        {
            // ARRANGE

            var category = this.categoryRepository.Upsert(DefaultCategory(this.categoryRepository.Root()));

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.True(result);
            Assert.Null(this.categoryRepository.FindById(category.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_nonempty_category_because_of_subcategory()
        {
            // ARRANGE

            var category = this.categoryRepository.Upsert(DefaultCategory(this.categoryRepository.Root()));
            var subCategory = this.categoryRepository.Upsert(DefaultCategory(category));

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.categoryRepository.FindById(category.Id));
            Assert.NotNull(this.categoryRepository.FindById(subCategory.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_nonempty_category_because_of_subentity()
        {
            // ARRANGE

            var category = this.categoryRepository.Upsert(DefaultCategory(this.categoryRepository.Root()));
            var entity = this.entityRepository.Upsert(DefaultEntity(WithCategory(category)));

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.categoryRepository.FindById(category.Id));
            Assert.NotNull(this.entityRepository.FindById(entity.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_root()
        {
            // ACT

            var result = this.traverser.DeleteIfEmpty(this.categoryRepository.Root());

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.categoryRepository.FindById(this.categoryRepository.Root().Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_removes_category_recursively()
        {
            // ARRANGE

            var category = this.categoryRepository.Upsert(DefaultCategory(this.categoryRepository.Root()));
            var subCategory = this.categoryRepository.Upsert(DefaultCategory(category));

            // ACT

            var result = this.traverser.DeleteRecursively(category);

            // ASSERT

            Assert.True(result);
            Assert.Null(this.categoryRepository.FindById(category.Id));
            Assert.Null(this.categoryRepository.FindById(subCategory.Id));
            Assert.Empty(this.categoryRepository.Root().SubCategories);
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_root_recursively()
        {
            // ACT

            var result = this.traverser.DeleteRecursively(this.categoryRepository.Root());

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.categoryRepository.FindById(this.categoryRepository.Root().Id));
        }
    }
}