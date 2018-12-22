using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public class RelationshipRepository : LiteDbRepositoryBase<Relationship>, IRelationshipRepository
    {
        public const string CollectionName = "relationships";
        private readonly IChangedMessageBus<IRelationship> eventSource;

        static RelationshipRepository()
        {
            BsonMapper.Global
               .Entity<Relationship>()
                   .DbRef(r => r.Tags, TagRepository.CollectionName)
                   .DbRef(r => r.From, EntityRepository.CollectionName)
                   .DbRef(r => r.To, EntityRepository.CollectionName);
        }

        public RelationshipRepository(LiteRepository db, Messaging.IChangedMessageBus<Messaging.IRelationship> eventSource) : base(db, CollectionName)
        {
            this.eventSource = eventSource;
        }

        public override Relationship Upsert(Relationship relationship)
        {
            this.eventSource.Modified(base.Upsert(relationship));
            return relationship;
        }

        public override bool Delete(Relationship relationship)
        {
            if (base.Delete(relationship))
            {
                this.eventSource.Removed(relationship);
                return true;
            }
            return false;
        }

        public override Relationship FindById(Guid id) => this.repository
            .Query<Relationship>(CollectionName)
            .Include(r => r.Tags)
            .Include(r => r.From)
            .Include(r => r.To)
            .SingleById(id);

        public override IEnumerable<Relationship> FindAll() => this.repository
            .Query<Relationship>(CollectionName)
            .Include(r => r.Tags)
            .Include(r => r.From)
            .Include(r => r.To)
            .ToEnumerable();
    }
}