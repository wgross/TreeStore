using Kosmograph.Messaging;
using LiteDB;
using System.IO;
using System.Linq;
using Xunit;
using static Kosmograph.LiteDb.Test.TestDataSources;

namespace Kosmograph.LiteDb.Test
{
    public class CategoryCopyTraverserTest
    {
        private readonly EntityRepository entityRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly CategoryCopyTraverser traverser;

        public CategoryCopyTraverserTest()
        {
            var tmp = new LiteRepositoryAdapater(new LiteRepository(new MemoryStream()));
            this.entityRepository = new EntityRepository(tmp.LiteRepository, KosmographMessageBus.Default.Entities);
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

            this.traverser.CopyCategory(src, dst);

            // ASSERT

            var srcRead = this.categoryRepository.FindById(src.Id);
            var dstRead = this.categoryRepository.FindById(dst.Id);

            Assert.Equal(src.Name, dstRead.SubCategories.Single().Name);
            Assert.NotEqual(src.Id, dstRead.SubCategories.Single().Id);
            Assert.Equal(root.Id, srcRead.Parent.Id);
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

            this.traverser.CopyCategory(src, dst);

            // ASSERT

            var src_read = this.categoryRepository.FindById(src.Id);
            var dst_read = this.categoryRepository.FindById(dst.Id);

            Assert.Equal(src.Name, dst_read.SubCategories.Single().Name);
            Assert.NotEqual(src.Id, src_read.SubCategories.Single().Id);

            var dst_src_read = this.categoryRepository.FindById(dst_read.SubCategories.Single().Id);

            Assert.Equal(src_sub.Name, dst_src_read.SubCategories.Single().Name);
            Assert.NotEqual(src_sub.Id, dst_src_read.SubCategories.Single().Id);
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

            this.traverser.CopyCategory(src, dst);

            // ASSERT

            var srcRead = this.categoryRepository.FindById(src.Id);
            var dst_src_read = this.categoryRepository.FindByCategoryAndName(dst, src.Name);
            var dst_src_entity_read = this.entityRepository.FindByCategoryAndName(dst_src_read, src_entity.Name);

            Assert.Equal(src_entity.Name, dst_src_entity_read.Name);
            Assert.NotEqual(src_entity.Id, dst_src_entity_read.Id);
        }
    }
}