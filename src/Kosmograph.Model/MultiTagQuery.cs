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

        public IEnumerable<TagQuery> TagQueries => this.tagQueries;

        public Action<Entity> EntityAdded { private get; set; }

        public Action<Entity> EntityChanged { private get; set; }

        public Action<Guid> EntityRemoved { private get; set; }

        public Action<Relationship> RelationshipAdded { private get; set; }

        public Action<Relationship> RelationshipChanged { private get; set; }

        public Action<Guid> RelationshipRemoved { private get; set; }

        #region Add tag query

        public TagQuery Add(Tag tag)
        {
            var tagQuery = new TagQuery(this.model, this.messageBus, tag);
            tagQuery.EntityAdded = this.OnEntityAdded;
            tagQuery.EntityRemoved = this.OnEntityRemoved;
            tagQuery.RelationshipAdded = this.OnRelationshipAdded;
            tagQuery.RelationshipRemoved = this.OnRelationshipRemoved;

            this.tagQueries.Add(tagQuery);
            tagQuery.StartQuery();
            return tagQuery;
        }

        private void OnEntityChanged(Entity obj) => this.EntityChanged?.Invoke(obj);

        private void OnRelationshipChanged(Relationship obj) => this.RelationshipChanged?.Invoke(obj);

        private void OnEntityAdded(Entity entity)
        {
            if (!this.tagQueries.Any(tq => tq.ContainsEntity(entity.Id)))
                this.EntityAdded?.Invoke(entity);
            else
                this.EntityChanged?.Invoke(entity);
        }

        private void OnEntityRemoved(Guid entityId)
        {
            if (this.tagQueries.Count(tq => tq.ContainsEntity(entityId)) == 1)
                this.EntityRemoved?.Invoke(entityId);
            else
                this.EntityChanged?.Invoke(this.model.Entities.FindById(entityId));
        }

        private void OnRelationshipAdded(Relationship relationship)
        {
            if (!this.tagQueries.Any(tq => tq.ContainsRelationship(relationship.Id)))
                this.RelationshipAdded?.Invoke(relationship);
            else
                this.RelationshipChanged?.Invoke(relationship);
        }

        private void OnRelationshipRemoved(Guid relationshipId)
        {
            if (this.tagQueries.Count(tq => tq.ContainsRelationship(relationshipId)) == 1)
                this.RelationshipRemoved?.Invoke(relationshipId);
            else
                this.RelationshipChanged?.Invoke(this.model.Relationships.FindById(relationshipId));
        }

        #endregion Add tag query

        public bool Contains(Tag tag) => this.tagQueries.FirstOrDefault(tq => tq.Tag.Equals(tag)) != null;

        public void Remove(TagQuery tagQuery)
        {
            tagQuery.StopQuery();
            this.tagQueries.Remove(tagQuery);
        }
    }
}