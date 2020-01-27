using TreeStore.Model;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.LiteDb
{
    /// <summary>
    ///  Deletion of a catwgeory is a cros collection operation.
    /// </summary>
    public sealed class CategoryRemovalTraverser
    {
        private CategoryRepository categoryRepository;
        private EntityRepository entityRepository;

        public CategoryRemovalTraverser(CategoryRepository categoryRepository, EntityRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        public bool DeleteIfEmpty(Category category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            if (SubCategories(category).Any())
                return false;

            if (SubEntites(category).Any())
                return false;

            return this.DeleteCategoryInDb(category);
        }

        #region Delete Recursive

        public bool DeleteRecursively(Category category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            // collect all entites and categories in the given parent category.
            // the delete them
            // This might be prpblement if the persistence is by two processes.
            // in these scenarios a persistence is required which will reject the operation.

            var entitiesToDelete = new List<Entity>();
            var categoriesToDelete = new List<Category>();

            this.CollectItemsToDelete(category, entitiesToDelete, categoriesToDelete);

            // delete the item from the DB
            foreach (var entityToDelete in entitiesToDelete)
                this.entityRepository.Delete(entityToDelete);
            foreach (var categoryToDelete in categoriesToDelete)
                this.DeleteCategoryInDb(categoryToDelete);
            return this.DeleteTopMostCategory(category);
        }

        private void CollectItemsToDelete(Category category, List<Entity> entitiesToDelete, List<Category> categoriesToDelete)
        {
            foreach (var subEntity in this.SubEntites(category))
            {
                entitiesToDelete.Add(subEntity);
            }

            foreach (var subCategory in SubCategories(category))
            {
                categoriesToDelete.Add(subCategory);
                this.CollectItemsToDelete(subCategory, entitiesToDelete, categoriesToDelete);
            }
        }

        private bool DeleteTopMostCategory(Category category)
        {
            // delete the category item from the low level collection
            if (this.DeleteCategoryInDb(category))
            {
                // remove the collection from the parent and update the database
                category.Parent.SubCategories.Remove(category);
                this.categoryRepository.Upsert(category.Parent);
                return true;
            }
            return false;
        }

        private bool DeleteCategoryInDb(Category category)
        {
            // do low level delete lite repos directly...
            if (!this.categoryRepository.LiteCollection().Delete(category.Id))
                return false;
            // if ok detach from the parent and upsert ist in the DB
            category.Parent.SubCategories.Remove(category);
            this.categoryRepository.LiteCollection<Category>().Upsert(category.Parent);
            return true;
        }

        #endregion Delete Resurvivly

        private IEnumerable<Entity> SubEntites(Category category) => this.entityRepository.FindByCategory(category);

        private IEnumerable<Category> SubCategories(Category category) => category.SubCategories;
    }
}