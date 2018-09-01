using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>
    {
        public TagRepository(LiteDatabase db) : base(db, "tags")
        {
        }
    }
}