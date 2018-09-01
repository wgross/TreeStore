using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>
    {
        public EntityRepository(LiteDatabase db) : base(db, "entities")
        {
        }
    }
}