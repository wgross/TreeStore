﻿using System.Linq;

namespace TreeStore.Model
{
    public class TagQuery : TrackingModelController
    {
        public Tag Tag { get; }

        public TagQuery(TreeStoreModel kosmographModel, Tag tag)
            : base(kosmographModel)
        {
            this.Tag = tag;
        }

        public void StartQuery()
        {
            this.Model
                .Entities
                .FindByTag(this.Tag)
                .ToList()
                .ForEach(e => this.AddEntity(e));

            this.Model
                .Relationships
                .FindByTag(this.Tag)
                .ToList()
                .ForEach(r => this.AddRelationship(r));
        }

        private void AddRelationship(Relationship added) => this.OnRelationshipChanging(added);

        private void AddEntity(Entity entity) => this.OnEntityChanging(entity);

        protected override void OnEntityAdding(Entity entity)
        {
            if (entity.Tags.Contains(this.Tag))
                base.OnEntityAdding(entity);
        }

        protected override void OnEntityChanging(Entity changed)
        {
            if (this.ContainsEntity(changed.Id))
            {
                if (!changed.Tags.Contains(this.Tag))
                {
                    // its a removal only if the entity is known and no longer contains the tag
                    RemoveEntity(changed);
                }
            }
            else if (changed.Tags.Contains(this.Tag))
            {
                // a yet unkonw entity having the query tags is
                // considered an "add" and is propagted to the tracking controller
                base.OnEntityChanging(changed);
            }
        }

        private void RemoveEntity(Entity entity) => this.OnEntityRemoving(entity.Id);

        protected override void OnRelationshipAdding(Relationship relationship)
        {
            if (relationship.Tags.Contains(this.Tag))
                base.OnRelationshipAdding(relationship);
        }

        protected override void OnRelationshipChanging(Relationship changing)
        {
            if (ContainsRelationship(changing.Id))
            {
                if (!changing.Tags.Contains(this.Tag))
                {
                    // its a removal only if the relationship is known and no longer contains the tag
                    this.RemoveRelationship(changing);
                }
            }
            else if (changing.Tags.Contains(this.Tag))
            {
                // a yet unkonw relationship having the query tags is
                // considered an "add" and is propagted to the tracking controller
                base.OnRelationshipChanging(changing);
            }
        }

        private void RemoveRelationship(Relationship removed) => this.OnRelationshipRemoving(removed.Id);

        public void StopQuery()
        {
            this.Dispose();

            foreach (var relationshipId in this.TrackedRelationships.ToArray())
                this.OnRelationshipRemoving(relationshipId);

            foreach (var entityId in this.TrackedEntities.ToArray())
                this.OnEntityRemoving(entityId);
        }
    }
}