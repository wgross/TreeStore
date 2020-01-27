using TreeStore.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.LiteDb
{
    public class CategoryRepository : LiteDbRepositoryBase, ICategoryRepository
    {
        static CategoryRepository()
        {
            BsonMapper.Global.Entity<Category>()
                .DbRef(c => c.SubCategories, "categories")
                .Ignore(c => c.Parent);
        }

        public CategoryRepository(LiteRepository db)
            : base(db, "categories")
        {
            this.rootNode = new Lazy<Category>(() => this.FindRootCategory() ?? this.CreateRootCategory());
        }

        #region There must be a persistent root

        private readonly Lazy<Category> rootNode;
        public static Guid CategoryRootId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public Category Root() => this.rootNode.Value;

        // todo: abandon completely loaded root tree
        private Category FindRootCategory() => this.LiteCollection<Category>().IncludeAll(maxDepth: 10).FindById(CategoryRootId);

        private Category CreateRootCategory() => this.Upsert(new Category(string.Empty) { Id = CategoryRootId });

        #endregion There must be a persistent root

        //public bool Delete(Category category, bool recurse = false)
        //{
        //    // check for children
        //    if (this.FindByCategory(category).Any())
        //        return false;

        //    if (!this.LiteCollection<Category>().Delete(category.Id))
        //        return false;

        //    category.Parent.SubCategories.Remove(category);
        //    this.Upsert(category.Parent);
        //    return true;
        //}

        public Category FindById(Guid id) => this.Root().FindSubCategory(id);

        public Category Upsert(Category category)
        {
            if (category.Parent is null && category.Id != CategoryRootId)
                throw new InvalidOperationException("Category must have parent.");

            if (this.LiteCollection<Category>().Upsert(category))
                if (category.Parent is { })
                    this.Upsert(category.Parent);

            return category;
        }

        public Category? FindByCategoryAndName(Category category, string name)
            => this.LiteCollection<Category>().IncludeAll(maxDepth: 1).FindById(category.Id)?.FindSubCategory(name, StringComparer.OrdinalIgnoreCase);

        public IEnumerable<Category> FindByCategory(Category category)
            => this.LiteCollection<Category>().IncludeAll(maxDepth: 1).FindById(category.Id)?.SubCategories ?? Enumerable.Empty<Category>();
    }
}