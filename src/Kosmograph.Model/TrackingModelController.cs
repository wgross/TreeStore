using Kosmograph.Messaging;
using System;
using System.Collections.Generic;

namespace Kosmograph.Model
{
    public class TrackingModelController : ModelController
    {
        private readonly HashSet<Guid> trackedTags = new HashSet<Guid>();
        private readonly HashSet<Guid> trackedEntities = new HashSet<Guid>();
        private readonly HashSet<Guid> trackedRelationships = new HashSet<Guid>();

        public TrackingModelController(KosmographModel model)
            : base(model)
        { }

        public TrackingModelController(IKosmographMessageBus kosmographMessageBus)
            : base(kosmographMessageBus)
        { }

        #region Track Tags

        public IEnumerable<Guid> TrackedTags => this.trackedTags;

        public bool ContainsTag(Tag tag) => this.trackedTags.Contains(tag.Id);

        public Action<Tag> TagAdded { private get; set; }

        #endregion Track Tags

        #region Track Entities

        public IEnumerable<Guid> TrackedEntities => this.trackedEntities;

        public bool ContainsEntity(Guid entityId) => this.trackedEntities.Contains(entityId);

        public Action<Entity> EntityAdded { private get; set; }

        #endregion Track Entities

        #region Track Relationships

        public IEnumerable<Guid> TrackedRelationships => this.trackedRelationships;

        public bool ContainsRelationship(Guid relationshipId) => this.trackedRelationships.Contains(relationshipId);

        public Action<Relationship> RelationshipAdded { private get; set; }

        #endregion Track Relationships

        #region Observe Tags

        protected override void OnChangingTag(Tag tag)
        {
            if (trackedTags.Contains(tag.Id))
                base.OnChangingTag(tag);
            else
                this.OnAddedTag(tag);
            this.trackedTags.Add(tag.Id);
        }

        private void OnAddedTag(Tag tag) => this.TagAdded?.Invoke(tag);

        protected override void OnRemovingTag(Guid tagId)
        {
            if (this.trackedTags.Contains(tagId))
                base.OnRemovingTag(tagId);
            this.trackedTags.Remove(tagId);
        }

        #endregion Observe Tags

        #region Observe Entities

        protected override void OnEntityChanging(Entity entity)
        {
            if (this.trackedEntities.Contains(entity.Id))
                base.OnEntityChanging(entity);
            else
                this.OnEntityAdding(entity);
            this.trackedEntities.Add(entity.Id);
        }

        protected virtual void OnEntityAdding(Entity entity) => this.EntityAdded?.Invoke(entity);

        protected override void OnEntityRemoving(Guid entityId)
        {
            if (this.trackedEntities.Contains(entityId))
                base.OnEntityRemoving(entityId);
            this.trackedEntities.Remove(entityId);
        }

        #endregion Observe Entities

        #region Observe Relationships

        protected override void OnRelationshipChanging(Relationship changed)
        {
            if (this.trackedRelationships.Contains(changed.Id))
                base.OnRelationshipChanging(changed);
            else
                this.OnRelationshipAdding(changed);
            this.trackedRelationships.Add(changed.Id);
        }

        protected virtual void OnRelationshipAdding(Relationship relationship) => this.RelationshipAdded?.Invoke(relationship);

        protected override void OnRelationshipRemoving(Guid relationshipId)
        {
            if (this.trackedRelationships.Contains(relationshipId))
                base.OnRelationshipRemoving(relationshipId);
            this.trackedRelationships.Remove(relationshipId);
        }

        #endregion Observe Relationships
    }
}