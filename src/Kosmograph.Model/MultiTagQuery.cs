using Kosmograph.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class MultiTagQuery
    {
        private readonly List<TagQuery> tagQueries = new List<TagQuery>();
        private readonly KosmographModel model;
        private readonly IKosmographMessageBus messageBus;

        public MultiTagQuery(KosmographModel model, IKosmographMessageBus messageBus)
        {
            this.model = model;
            this.messageBus = messageBus;
        }

        public Action<Entity> EntityAdded { private get; set; }

        public Action<Entity> EntityChanged { private get; set; }

        public Action<Guid> EntityRemoved { private get; set; }

        public Action<Relationship> RelationshipAdded { private get; set; }

        public Action<Relationship> RelationshipChanged { private get; set; }

        public Action<Guid> RelationshipRemoved { private get; set; }

        public void Add(Tag tag)
        {
            var tagQuery = new TagQuery(this.model, this.messageBus, tag);
            tagQuery.EntityAdded = this.OnEntityAdded;
            tagQuery.EntityRemoved = this.OnEntityRemoved;
            tagQuery.EntityChanged = this.OnEntityChanged;
            tagQuery.RelationshipAdded = this.OnRelationshipAdded;
            tagQuery.RelationshipRemoved = this.OnRelationshipRemoved;
            tagQuery.RelationshipChanged = this.OnRelationshipChanged;

            this.tagQueries.Add(tagQuery);
            tagQuery.StartQuery();
        }

        private void OnEntityChanged(Entity obj) => this.EntityChanged?.Invoke(obj);

        private void OnRelationshipChanged(Relationship obj) => this.RelationshipChanged?.Invoke(obj);

        private void OnEntityAdded(Entity entity)
        {
            // notfication is sent for the first query only
            if (!this.tagQueries.Any(tq => tq.ContainsEntity(entity.Id)))
                this.EntityAdded?.Invoke(entity);
        }

        private void OnEntityRemoved(Guid entityId)
        {
            // notfication is sent for the last query only
            if (this.tagQueries.Count(tq => tq.ContainsEntity(entityId)) == 1)
                this.EntityRemoved?.Invoke(entityId);
        }

        private void OnRelationshipAdded(Relationship relationship)
        {
            if (!this.tagQueries.Any(tq => tq.ContainsRelationship(relationship.Id)))
                this.RelationshipAdded?.Invoke(relationship);
        }

        private void OnRelationshipRemoved(Guid relationshipId)
        {
            // notfication is sent for the last query only
            if (this.tagQueries.Count(tq => tq.ContainsRelationship(relationshipId)) == 1)
                this.RelationshipRemoved?.Invoke(relationshipId);
        }

        public bool Contains(Tag tag) => this.tagQueries.FirstOrDefault(tq => tq.Tag.Equals(tag)) != null;
    }
}