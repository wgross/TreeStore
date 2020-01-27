﻿using TreeStore.Messaging;
using TreeStore.Model;
using LiteDB;

namespace TreeStore.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>, ITagRepository
    {
        public const string CollectionName = "tags";

        // private readonly FacetRepository facets;
        private readonly IChangedMessageBus<ITag> eventSource;

        public TagRepository(LiteRepository repo, IChangedMessageBus<Messaging.ITag> eventSource) : base(repo, CollectionName)
        {
            repo.Database
                .GetCollection(CollectionName)
                .EnsureIndex(field: nameof(Tag.Name), expression: $"LOWER($.{nameof(Tag.Name)})", unique: true);

            this.eventSource = eventSource;
        }

        public override Tag Upsert(Tag entity)
        {
            this.eventSource.Modified(base.Upsert(entity));
            return entity;
        }

        public override bool Delete(Tag tag)
        {
            if (base.Delete(tag))
            {
                this.eventSource.Removed(tag);
                return true;
            }
            return false;
        }

        public Tag FindByName(string name) => this.LiteRepository.Query<Tag>(CollectionName).Where(t => t.Name.Equals(name)).FirstOrDefault();
    }
}