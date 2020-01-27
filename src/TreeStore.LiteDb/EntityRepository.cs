﻿using TreeStore.Messaging;
using TreeStore.Model;
using LiteDB;
using System;
using System.Collections.Generic;

namespace TreeStore.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>, IEntityRepository
    {
        private readonly IChangedMessageBus<IEntity> eventSource;

        static EntityRepository()
        {
            BsonMapper.Global
                .Entity<Entity>()
                    .DbRef(e => e.Tags, TagRepository.CollectionName)
                    .DbRef(e => e.Category, "categories");
        }

        public EntityRepository(LiteRepository db, IChangedMessageBus<IEntity> eventSource) : base(db, "entities")
        {
            db.Database
                .GetCollection(CollectionName)
                .EnsureIndex(field: nameof(Entity.UniqueName), expression: $"$.{nameof(Entity.UniqueName)}", unique: true);

            this.eventSource = eventSource;
        }

        public override Entity Upsert(Entity entity)
        {
            this.eventSource.Modified(base.Upsert(entity));
            return entity;
        }

        public override bool Delete(Entity entity)
        {
            var relationshipExists = this.LiteRepository
                .Query<Relationship>("relationships")
                .Where(r => r.From.Id.Equals(entity.Id) || r.To.Id.Equals(entity.Id))
                .Exists();

            if (relationshipExists)
                return false;

            if (base.Delete(entity))
            {
                this.eventSource.Removed(entity);
                return true;
            }
            return false;
        }

        public override Entity FindById(Guid id) => this.LiteRepository
            .Query<Entity>(CollectionName)
            .Include(e => e.Tags)
            .SingleById(id);

        public override IEnumerable<Entity> FindAll() => this.LiteRepository
            .Query<Entity>(CollectionName)
            .Include(e => e.Tags)
            .ToArray();

        public IEnumerable<Entity> FindByTag(Tag tag)
        {
            // i'm sure this is a table scan...
            return this.LiteRepository.Query<Entity>(CollectionName)
                .Include(e => e.Tags)
                .Where(e => e.Tags.Contains(tag))
                .ToArray();
        }

        public Entity? FindByName(string name) => this.LiteRepository
            .Query<Entity>(CollectionName)
            .Include(e => e.Tags)
            .Where(e => e.Name.Equals(name))
            .FirstOrDefault();

        public IEnumerable<Entity> FindByCategory(Category category)
        {
            return this.LiteRepository
                .Query<Entity>(CollectionName)
                .Include(e => e.Tags)
                .Where(e => e.Category.Id == category.Id)
                .ToArray();
        }

        public Entity? FindByCategoryAndName(Category category, string name)
        {
            return this.LiteRepository
                .Query<Entity>(CollectionName)
                .Include(e => e.Tags)
                .Where(e => e.Category.Id == category.Id && e.Name.Equals(name))
                .FirstOrDefault();
        }
    }
}