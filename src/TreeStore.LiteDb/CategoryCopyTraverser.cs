using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class CategoryCopyTraverser
    {
        private CategoryRepository categoryRepository;
        private EntityRepository entityRepository;

        public CategoryCopyTraverser(CategoryRepository categoryRepository, EntityRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        public void CopyCategory(Category src, Category dst)
        {
            // copy the top most src as child of the dst
            var srcClone = CloneWithNewId(src);
            dst.AddSubCategory(srcClone);
            this.categoryRepository.Upsert(srcClone);
            this.categoryRepository.Upsert(dst);
            // descend n src and continue wioth sub categories
            foreach (var srcChild in src.SubCategories)
            {
                CopyCategory(srcChild, srcClone);
            }
            // copy all enities in src to dst
            foreach (var srcEntity in this.entityRepository.FindByCategory(src))
            {
                CopyEntity(srcEntity, srcClone);
            }
        }

        private void CopyEntity(Entity srcEntity, Category dstCategory)
        {
            var srcClone = CloneWithNewId(srcEntity);
            srcClone.SetCategory(dstCategory);
            this.entityRepository.Upsert(srcClone);
        }

        private Category CloneWithNewId(Category category) => (Category)category.Clone();

        private Entity CloneWithNewId(Entity entity) => (Entity)entity.Clone();
    }
}