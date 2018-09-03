using Kosmograph.Model;
using LiteDB;
using System;

namespace Kosmograph.LiteDb
{
    public class CategoryRepository : ICategoryRepository
    {
        public const string CollectionName = "categories";

        private readonly LiteRepository liteDb;

        static CategoryRepository()
        {
            BsonMapper.Global.Entity<Category>()
                .DbRef(c => c.SubCategories, CollectionName)
                .Ignore(c => c.Parent);
        }

        public CategoryRepository(LiteRepository db)
        {
            this.liteDb = db;
        }

        #region There must be a persistent root

        private static readonly Guid CategoryRootId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public Category Root() => this.FindRootCategory() ?? this.CreateRootCategory();

        private Category FindRootCategory() => this.liteDb.Database.GetCollection<Category>(CollectionName).IncludeAll(maxDepth: 10).FindById(CategoryRootId);

        private Category CreateRootCategory() => this.Upsert(new Category(string.Empty, Facet.Empty) { Id = CategoryRootId });

        #endregion There must be a persistent root

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public Category FindById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Category Upsert(Category category)
        {
            if (category.Parent is null && !category.Id.Equals(CategoryRootId))
                throw new InvalidOperationException("Category must have parent.");

            this.liteDb.Upsert(category, CollectionName);
            return category;
        }
    }
}