using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public RelationshipRepository(LiteRepository repo, Messaging.IChangedMessageBus<Messaging.IRelationship> eventSource) : base(repo, CollectionName)
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

        public void Delete(IEnumerable<Relationship> relationships)
            => relationships.ToList().ForEach(r => this.Delete(r));

        public override Relationship FindById(Guid id) => this.QueryAndInclude().SingleById(id);

        public override IEnumerable<Relationship> FindAll() => this.QueryAndInclude().ToEnumerable();

        public IEnumerable<Relationship> FindByEntity(Entity entity) => this.QueryAndInclude(q => q.Where(r => r.From.Id.Equals(entity.Id) || r.To.Id.Equals(entity.Id))).ToEnumerable();

        public IEnumerable<Relationship> FindByTag(Tag tag) => this.QueryAndInclude(q => q.Where(r => r.Tags.Contains(tag))).ToEnumerable();

        private LiteQueryable<Relationship> QueryAndInclude(Func<LiteQueryable<Relationship>, LiteQueryable<Relationship>> query = null)
        {
            if (query is null)
                return QueryAndInclude(q => q);

            return query(this.Repository.Query<Relationship>(RelationshipRepository.CollectionName))
                .Include(r => r.Tags)
                .Include(r => r.From)
                .Include(r => r.To);
        }

        
        
    }
}