using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class CategoryRepository : LiteDbRepositoryBase, ICategoryRepository
    {
        static CategoryRepository()
        {
            BsonMapper.Global.Entity<Category>()
                .DbRef(c => c.Parent, "categories");
        }

        public CategoryRepository(LiteRepository db)
            : base(db, collectionName: "categories")
        {
            db.Database
                .GetCollection(CollectionName)
                .EnsureIndex(field: nameof(Category.UniqueName), expression: $"$.{nameof(Category.UniqueName)}", unique: true);

            this.rootNode = new Lazy<Category>(() => this.FindRootCategory() ?? this.CreateRootCategory());
        }

        #region There must be a persistent root

        private readonly Lazy<Category> rootNode;

        public Category Root() => this.rootNode.Value;

        // todo: abandon completely loaded root tree
        private Category FindRootCategory() => this.LiteRepository
                .Query<Category>(this.CollectionName)
                .Include(c => c.Parent)
                .Where(c => c.Parent == null)
                .FirstOrDefault();

        private Category CreateRootCategory()
        {
            var rootCategory = new Category(string.Empty);
            this.LiteCollection<Category>().Upsert(rootCategory);
            return rootCategory;
        }

        #endregion There must be a persistent root

        public Category FindById(Guid id) => this.LiteCollection<Category>()
            .Include(c => c.Parent)
            .FindById(id);

        public Category Upsert(Category category)
        {
            if (category.Parent is null)
                throw new InvalidOperationException("Category must have parent.");

            this.LiteCollection<Category>().Upsert(category);

            return category;
        }

        public Category? FindByParentAndName(Category category, string name) => this.FindByParent(category)
            .SingleOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); // matched in result set

        public IEnumerable<Category> FindByParent(Category category)
        {
            return this.LiteRepository
                .Query<Category>(this.CollectionName)
                .Include(c => c.Parent)
                .Where(c => c.Parent != null && c.Parent.Id == category.Id)
                .ToArray();
        }
    }
}