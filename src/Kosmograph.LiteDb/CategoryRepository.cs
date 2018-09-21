using Kosmograph.Model;
using LiteDB;
using System;

namespace Kosmograph.LiteDb
{
    public class CategoryRepository : ICategoryRepository
    {
        public const string CollectionName = "categories";

        private readonly LiteRepository liteDb;
        private readonly Lazy<Category> rootNode;

        static CategoryRepository()
        {
            BsonMapper.Global.Entity<Category>()
                .DbRef(c => c.SubCategories, CollectionName)
                .Ignore(c => c.Parent);
        }

        public CategoryRepository(LiteRepository db)
        {
            this.liteDb = db;
            this.rootNode = new Lazy<Category>(() => this.FindRootCategory() ?? this.CreateRootCategory());
        }

        #region There must be a persistent root

        private static readonly Guid CategoryRootId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public Category Root() => this.rootNode.Value;

        private Category FindRootCategory() => this.liteDb.Database.GetCollection<Category>(CollectionName).IncludeAll(maxDepth: 10).FindById(CategoryRootId);

        private Category CreateRootCategory() => this.Upsert(new Category(string.Empty) { Id = CategoryRootId });

        #endregion There must be a persistent root

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public Category FindById(Guid id) => this.Root().FindSubCategory(id);

        public Category Upsert(Category category)
        {
            if (category.Parent is null && category.Id != CategoryRootId)
                throw new InvalidOperationException("Category must have parent.");

            if (this.liteDb.Upsert(category, CollectionName))
                if (!(category.Parent is null))
                    this.Upsert(category.Parent);
            return category;
        }
    }
}