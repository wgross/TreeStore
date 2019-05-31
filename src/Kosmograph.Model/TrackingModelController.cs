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

        public TrackingModelController(IKosmographMessageBus kosmographMessageBus)
            : base(kosmographMessageBus.Tags, kosmographMessageBus.Entities, kosmographMessageBus.Relationships)
        { }

        public TrackingModelController(IChangedMessageBus<ITag> tags, IChangedMessageBus<IEntity> entities, IChangedMessageBus<IRelationship> relationships)
            : base(tags, entities, relationships)
        { }

        public bool Contains(Tag tag) => this.trackedTags.Contains(tag.Id);

        public Action<Tag> TagAdded { private get; set; }

        public bool ContainsEntity(Guid entityId) => this.trackedEntities.Contains(entityId);

        public Action<Entity> EntityAdded { private get; set; }

        public bool ContainsRelationship(Guid relationshipId) => this.trackedRelationships.Contains(relationshipId);

        public Action<Relationship> RelationshipAdded { private get; set; }

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

        protected override void OnChangingEntity(Entity entity)
        {
            if (trackedEntities.Contains(entity.Id))
                base.OnChangingEntity(entity);
            else
                this.OnAddedEntity(entity);
            this.trackedEntities.Add(entity.Id);
        }

        protected virtual void OnAddedEntity(Entity entity) => this.EntityAdded?.Invoke(entity);

        protected override void OnRemovingEntity(Guid entityId)
        {
            if (this.trackedEntities.Contains(entityId))
                base.OnRemovingEntity(entityId);
            this.trackedEntities.Remove(entityId);
        }

        #endregion Observe Entities

        #region Observe Relationships

        protected override void OnChangingRelationship(Relationship changed)
        {
            if (this.trackedRelationships.Contains(changed.Id))
                base.OnChangingRelationship(changed);
            else
                this.OnAddedRelationship(changed);
            this.trackedRelationships.Add(changed.Id);
        }

        protected virtual void OnAddedRelationship(Relationship relationship) => this.RelationshipAdded?.Invoke(relationship);

        protected override void OnRemovingRelationship(Guid relationshipId)
        {
            if (this.trackedRelationships.Contains(relationshipId))
                base.OnRemovingRelationship(relationshipId);
            this.trackedRelationships.Remove(relationshipId);
        }

        #endregion Observe Relationships
    }
}