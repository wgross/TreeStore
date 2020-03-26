using LiteDB;
using System.IO;
using System.Linq;
using TreeStore.Messaging;
using Xunit;
using static TreeStore.LiteDb.Test.TestDataSources;

namespace TreeStore.LiteDb.Test
{
    public class CategoryCopyTraverserTest
    {
        private readonly EntityRepository entityRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly CategoryCopyTraverser traverser;

        public CategoryCopyTraverserTest()
        {
            var tmp = new LiteRepositoryAdapater(new LiteRepository(new MemoryStream()));
            this.entityRepository = new EntityRepository(tmp.LiteRepository, TreeStoreMessageBus.Default.Entities);
            this.categoryRepository = new CategoryRepository(tmp.LiteRepository);
            this.traverser = new CategoryCopyTraverser(this.categoryRepository, this.entityRepository);
        }

        [Fact]
        public void CategoryCopyTraverser_copies_empty_category()
        {
            // ARRANGE

            var root = this.categoryRepository.Root();
            var src = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "src"));
            var dst = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "dst"));

            // ACT

            this.traverser.CopyCategoryRecursively(src, dst);

            // ASSERT

            var assert_src = this.categoryRepository.FindById(src.Id);
            var assert_dst = this.categoryRepository.FindById(dst.Id);
            var assert_dst_children = this.categoryRepository.FindByParent(dst);

            Assert.Equal(src.Name, assert_dst_children.Single().Name);
            Assert.NotEqual(src.Id, assert_dst_children.Single().Id);
            Assert.Equal(root.Id, assert_src.Parent.Id);
        }

        [Fact]
        public void CategoryCopyTraverser_copies_category_with_subcategory()
        {
            // ARRANGE

            var root = this.categoryRepository.Root();
            var src = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "src"));
            var src_sub = this.categoryRepository.Upsert(DefaultCategory(src, c => c.Name = "src-sub"));
            var dst = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "dst"));

            // ACT

            this.traverser.CopyCategoryRecursively(src, dst);

            // ASSERT

            var assert_dst_children = this.categoryRepository.FindByParent(dst);
            var assert_dst_src = this.categoryRepository.FindById(assert_dst_children.Single().Id);
            var assert_dst_src_children = this.categoryRepository.FindByParent(assert_dst_src);

            Assert.Equal(src.Name, assert_dst_src.Name);
            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src_sub.Name, assert_dst_src_children.Single().Name);
            Assert.NotEqual(src_sub.Id, assert_dst_src_children.Single().Id);
        }

        [Fact]
        public void CategoryCopyTraverser_copies_category_with_entity()
        {
            // ARRANGE

            var root = this.categoryRepository.Root();
            var src = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "src"));
            var dst = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "dst"));
            var src_entity = this.entityRepository.Upsert(DefaultEntity(WithEntityCategory(src)));

            // ACT

            this.traverser.CopyCategoryRecursively(src, dst);

            // ASSERT

            var assert_src = this.categoryRepository.FindById(src.Id);
            var assert_dst_src = this.categoryRepository.FindByParentAndName(dst, src.Name);
            var assert_dst_src_entity = this.entityRepository.FindByCategoryAndName(assert_dst_src, src_entity.Name);

            Assert.Equal(src_entity.Name, assert_dst_src_entity.Name);
            Assert.NotEqual(src_entity.Id, assert_dst_src_entity.Id);
        }

        [Fact]
        public void CategoryCopyTraverser_copies_category_without_items()
        {
            // ARRANGE

            var root = this.categoryRepository.Root();
            var src = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "src"));
            var src_sub = this.categoryRepository.Upsert(DefaultCategory(src, c => c.Name = "src-sub"));
            var dst = this.categoryRepository.Upsert(DefaultCategory(root, c => c.Name = "dst"));

            // ACT

            this.traverser.CopyCategory(src, dst);

            // ASSERT

            var assert_src = this.categoryRepository.FindById(src.Id);
            var assert_src_sub = this.categoryRepository.FindByParent(src);
            var assert_dst = this.categoryRepository.FindById(dst.Id);
            var assert_dst_children = this.categoryRepository.FindByParent(dst);
            var assert_dst_src_children = this.categoryRepository.FindByParent(assert_dst_children.Single());

            Assert.Equal(src.Name, assert_dst_children.Single().Name);
            Assert.Empty(assert_dst_src_children);
        }
    }
}