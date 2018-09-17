using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public class RelationshipRepository : LiteDbRepositoryBase<Relationship>, IRelationshipRepository
    {
        public const string CollectionName = "relationships";

        static RelationshipRepository()
        {
            BsonMapper.Global
               .Entity<Relationship>()
                   .DbRef(r => r.Tags, TagRepository.CollectionName)
                   .DbRef(r => r.From, EntityRepository.CollectionName)
                   .DbRef(r => r.To, EntityRepository.CollectionName);
        }

        public RelationshipRepository(LiteRepository db) : base(db, CollectionName)
        {
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