using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Kosmograph.LiteDb
{
    public class TagQuery : IObserver<ChangedMessage<IEntity>>, IObserver<ChangedMessage<IRelationship>>
    {
        private readonly KosmographModel persistence;
        private readonly KosmographMessageBus messageBus;
        private readonly Tag tag;
        private IDisposable entitiesSubscription;
        private IDisposable relationshipsSubscription;
        private readonly HashSet<Guid> knownEntityIds = new HashSet<Guid>();
        private readonly HashSet<Guid> knownRelationshipIds = new HashSet<Guid>();

        public TagQuery(KosmographModel model, KosmographMessageBus messageBus, Tag tag)
        {
            this.persistence = model;
            this.messageBus = messageBus;
            this.tag = tag;
        }

        public void StartQuery()
        {
            this.persistence
                .Entities
                .FindByTag(this.tag)
                .ToList()
                .ForEach(e => this.AddEntity(e));

            this.persistence
               .Relationships
               .FindByTag(this.tag)
               .ToList()
               .ForEach(r => this.AddRelationship(r));

            this.entitiesSubscription = this.messageBus.Entities.Subscribe(this);
            this.relationshipsSubscription = this.messageBus.Relationships.Subscribe(this);
        }

        public IEnumerable<Entity> GetEntities()
        {
            var tmp = this.persistence.Entities.FindByTag(this.tag)
                .Where(e => !this.knownEntityIds.Contains(e.Id))
                .ToList();
            tmp.ForEach(e => this.knownEntityIds.Add(e.Id));
            return tmp;
        }

        public IEnumerable<Relationship> GetRelationships()
        {
            return this.persistence.Relationships.FindByTag(this.tag);
        }

        #region Add or remove Entites from result set

        void IObserver<ChangedMessage<IEntity>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IEntity>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IEntity>>.OnNext(ChangedMessage<IEntity> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Modified:
                    this.AddEntity(this.persistence.Entities.FindById(value.Changed.Id));
                    break;

                case ChangeTypeValues.Removed:
                    // an unknown entity isn't notified
                    if (!this.knownEntityIds.Contains(value.Changed.Id))
                        break;
                    this.EntityRemoved?.Invoke(value.Changed.Id);
                    break;
            }
        }

        private void AddEntity(Entity entity)
        {
            // a known entity isn't notified again
            if (this.knownEntityIds.Contains(entity.Id))
            {
                if (!entity.Tags.Contains(this.tag))
                    this.EntityRemoved?.Invoke(entity.Id);

                this.knownEntityIds.Remove(entity.Id);
            }
            else
            {
                // if it contains the tag ist is added to the result set
                if (entity.Tags.Contains(this.tag))
                    this.EntityAdded?.Invoke(entity);

                // after notification the entity is classified as 'known'
                this.knownEntityIds.Add(entity.Id);
            }
        }

        public Action<Entity> EntityAdded { private get; set; }

        public Action<Guid> EntityRemoved { private get; set; }

        #endregion Add or remove Entites from result set

        #region Add or remove Relationships from result set

        void IObserver<ChangedMessage<IRelationship>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IRelationship>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IRelationship>>.OnNext(ChangedMessage<IRelationship> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Modified:
                    this.AddRelationship(this.persistence.Relationships.FindById(value.Changed.Id));
                    break;

                case ChangeTypeValues.Removed:
                    // an unknown relationship isn't notified
                    if (!this.knownRelationshipIds.Contains(value.Changed.Id))
                        break;
                    this.RelationshipRemoved?.Invoke(value.Changed.Id);
                    break;
            }
        }

        private void AddRelationship(Relationship relationship)
        {
            // a known entity isn't notified again
            if (this.knownRelationshipIds.Contains(relationship.Id))
            {
                if (!relationship.Tags.Contains(this.tag))
                    this.RelationshipRemoved?.Invoke(relationship.Id);

                this.knownRelationshipIds.Remove(relationship.Id);
            }
            else
            {
                // if it contains the tag ist is added to the result set
                if (relationship.Tags.Contains(this.tag))
                    this.RelationshipAdded?.Invoke(relationship);

                // after notification the entity is classified as 'known'
                this.knownRelationshipIds.Add(relationship.Id);
            }
        }

        public Action<Relationship> RelationshipAdded { private get; set; }

        public Action<Guid> RelationshipRemoved { private get; set; }

        #endregion Add or remove Relationshios from result set
    }
}