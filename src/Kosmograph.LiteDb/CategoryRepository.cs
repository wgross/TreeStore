using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class CategoryRepository : LiteDbRepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(LiteRepository db) : base(db, "categories")
        {
        }

        public Category Root()
        {
            throw new System.NotImplementedException();
        }
    }
}