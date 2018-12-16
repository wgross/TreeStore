using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>, IEntityRepository
    {
        public const string CollectionName = "entities";
        private readonly IChangedMessageBus<IEntity> eventSoure;

        static EntityRepository()
        {
            BsonMapper.Global
                .Entity<Entity>()
                    .DbRef(e => e.Tags, TagRepository.CollectionName)
                    .DbRef(e => e.Category, CategoryRepository.CollectionName);
        }

        public EntityRepository(LiteRepository db, IChangedMessageBus<IEntity> eventSource) : base(db, CollectionName)
        {
            this.eventSoure = eventSource;
        }

        public override Entity Upsert(Entity entity)
        {
            this.eventSoure.Modified(base.Upsert(entity));
            return entity;
        }

        public override bool Delete(Entity entity)
        {
            if (base.Delete(entity))
            {
                this.eventSoure.Removed(entity);
                return true;
            }
            return false;
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