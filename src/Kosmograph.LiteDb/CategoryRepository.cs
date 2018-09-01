using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class CategoryRepository : LiteDbRepositoryBase<Category>
    {
        public CategoryRepository(LiteDatabase db) : base(db, "categories")
        {
        }
    }
}