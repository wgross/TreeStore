using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class CategoryRepository : LiteDbRepositoryBase<Category>, ICategoryRepository
    {
        public const string CollectionName = "categories";

        static CategoryRepository()
        {
            BsonMapper.Global.Entity<Category>()
                .DbRef(c => c.Parent, "categories");
        }

        public CategoryRepository(LiteRepository db)
            : base(db, collectionName: CollectionName)
        {
            db.Database
                .GetCollection(CollectionName)
                .EnsureIndex(
                    name: nameof(Category.UniqueName),
                    expression: $"$.{nameof(Category.UniqueName)}",
                    unique: true);

            this.rootNode = new Lazy<Category>(() => this.FindRootCategory() ?? this.CreateRootCategory());
        }

        protected override ILiteCollection<Category> IncludeRelated(ILiteCollection<Category> from) => from.Include(c => c.Parent);

        private ILiteQueryable<Category> QueryRelated() => this.LiteCollection().Query().Include(c => c.Parent);

        #region There must be a persistent root

        private readonly Lazy<Category> rootNode;

        public Category Root() => this.rootNode.Value;

        // todo: abandon completely loaded root tree
        private Category FindRootCategory() => this.LiteRepository
                .Query<Category>(CollectionName)
                .Include(c => c.Parent)
                .Where(c => c.Parent == null)
                .FirstOrDefault();

        private Category CreateRootCategory()
        {
            var rootCategory = new Category(string.Empty);
            this.LiteCollection().Upsert(rootCategory);
            return rootCategory;
        }

        #endregion There must be a persistent root

        public override Category Upsert(Category category)
        {
            if (category.Parent is null)
                throw new InvalidOperationException("Category must have parent.");

            return base.Upsert(category);
        }

        public Category? FindByParentAndName(Category category, string name)
        {
            return this
                .FindByParent(category)
                // matched in result set, could be mathed to expression
                .SingleOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Category> FindByParent(Category category)
        {
            return this.QueryRelated()
                .Where(c => c.Parent != null && c.Parent.Id == category.Id)
                .ToArray();
        }
    }
}