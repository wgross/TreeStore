using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>, IEntityRepository
    {
        public const string CollectionName = "entities";

        static EntityRepository()
        {
            BsonMapper.Global
                .Entity<Entity>()
                    .DbRef(e => e.Tags, TagRepository.CollectionName)
                    .DbRef(e => e.Category, CategoryRepository.CollectionName);
        }

        public EntityRepository(LiteRepository db) : base(db, CollectionName)
        {
        }

        public override Entity FindById(Guid id) => this.repository
            .Query<Entity>(CollectionName)
            .Include(e => e.Tags)
            .SingleById(id);

        public override IEnumerable<Entity> FindAll() => this.repository
            .Query<Entity>(CollectionName)
            .Include(e => e.Tags)
            .ToArray();
    }
}