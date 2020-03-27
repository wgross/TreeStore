using LiteDB;
using System;
using System.Collections.Generic;
using TreeStore.Messaging;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class EntityRepository : LiteDbRepositoryBase<Entity>, IEntityRepository
    {
        public const string CollectionName = "entities";

        private readonly IChangedMessageBus<IEntity> eventSource;

        static EntityRepository()
        {
            BsonMapper.Global
                .Entity<Entity>()
                    .DbRef(e => e.Tags, TagRepository.CollectionName)
                    .DbRef(e => e.Category, CategoryRepository.CollectionName);
        }

        public EntityRepository(LiteRepository db, IChangedMessageBus<IEntity> eventSource) : base(db, CollectionName)
        {
            db.Database
                .GetCollection(CollectionName)
                .EnsureIndex(
                    name: nameof(Entity.UniqueName),
                    expression: $"$.{nameof(Entity.UniqueName)}",
                    unique: true);

            db.Database
                .GetCollection<Entity>(CollectionName)
                .EnsureIndex(e => e.Category);

            this.eventSource = eventSource;
        }

        protected override ILiteCollection<Entity> IncludeRelated(ILiteCollection<Entity> from) => from.Include(e => e.Tags);

        protected ILiteQueryable<Entity> QueryRelated() => this.LiteCollection().Query().Include(e => e.Tags).Include(e => e.Category);

        public override Entity Upsert(Entity entity)
        {
            if (entity.Category is null)
                throw new InvalidOperationException("Entity must have category.");

            this.eventSource.Modified(base.Upsert(entity));
            return entity;
        }

        public override bool Delete(Entity entity)
        {
            var relationshipExists = this.LiteRepository
                .Query<Relationship>("relationships")
                .Include(r => r.From)
                .Include(r => r.To)
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

        public IEnumerable<Entity> FindByTag(Tag tag) => this.QueryRelated()
            // todo: optimize
            // i'm sure this is a table scan...LiteDb 5 may index that?
            .Where(e => e.Tags.Contains(tag))
            .ToArray();

        public IEnumerable<Entity> FindByCategory(Category category) => this.QueryRelated()
            .Where(e => e.Category.Id == category.Id)
            .ToArray();

        //public IEnumerable<Entity> FindByCategory(Category category) => this.QueryRelated()
        //    .Where(e => e.Category != null && e.Category.Id == category.Id)
        //    .ToArray();

        public Entity? FindByCategoryAndName(Category category, string name) => this.QueryRelated()
            .Where(e => e.Category.Id == category.Id && e.Name.Equals(name))
            .FirstOrDefault();
    }
}