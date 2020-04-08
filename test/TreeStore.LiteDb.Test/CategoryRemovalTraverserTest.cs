using TreeStore.Model;
using Moq;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class CategoryRemovalTraverserTest : LiteDbTestBase
    {
        private readonly CategoryRemovalTraverser traverser;

        public CategoryRemovalTraverserTest()
        {
            this.traverser = new CategoryRemovalTraverser(this.CategoryRepository, this.EntityRepository);
        }

        [Fact]
        public void CategoryRemovalTraverser_removes_empty_category()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.True(result);
            Assert.Null(this.CategoryRepository.FindById(category.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_nonempty_category_because_of_subcategory()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var subCategory = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(category)));

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(category.Id));
            Assert.NotNull(this.CategoryRepository.FindById(subCategory.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_nonempty_category_because_of_subentity()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());

            this.EntityEventSource
                .Setup(ev => ev.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT

            var result = this.traverser.DeleteIfEmpty(category);

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(category.Id));
            Assert.NotNull(this.EntityRepository.FindById(entity.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_root()
        {
            // ACT

            var result = this.traverser.DeleteIfEmpty(this.CategoryRepository.Root());

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(this.CategoryRepository.Root().Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_removes_category_recursively()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var subCategory = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(category)));

            // ACT

            var result = this.traverser.DeleteRecursively(category);

            // ASSERT

            Assert.True(result);
            Assert.Null(this.CategoryRepository.FindById(category.Id));
            Assert.Null(this.CategoryRepository.FindById(subCategory.Id));
        }

        [Fact]
        public void CategoryRemovalTraverser_rejects_removing_of_root_recursively()
        {
            // ACT

            var result = this.traverser.DeleteRecursively(this.CategoryRepository.Root());

            // ASSERT

            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(this.CategoryRepository.Root().Id));
        }
    }
}