using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>
    {
        public const string CollectionName = "entities";

        static EntityRepository()
        {
            BsonMapper.Global
                .Entity<Entity>()
                    .DbRef(e => e.Tags, TagRepository.CollectionName);
        }

        public EntityRepository(LiteRepository db) : base(db, CollectionName)
        {
        }

        public override Entity FindById(BsonValue id)
        {
            return this.repository.Query<Entity>(CollectionName).Include(e => e.Tags).SingleById(id);
        }
    }
}