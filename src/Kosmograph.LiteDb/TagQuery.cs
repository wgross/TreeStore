using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Kosmograph.LiteDb
{
    public class TagQuery : IObserver<ChangedMessage<IEntity>>
    {
        private readonly KosmographModel persistence;
        private readonly KosmographMessageBus messageBus;
        private readonly Tag tag;
        private readonly Subject<Entity> entitiesSubject;
        private readonly HashSet<Guid> entityIds = new HashSet<Guid>();

        public TagQuery(KosmographModel model, KosmographMessageBus messageBus, Tag tag)
        {
            this.persistence = model;
            this.messageBus = messageBus;
            this.tag = tag;
            this.messageBus.Entities.Subscribe(this);
            this.entitiesSubject = new Subject<Entity>();
        }

        public IEnumerable<Entity> GetEntities()
        {
            return this.persistence.Entities.FindByTag(this.tag)
                .Where(e => !this.entityIds.Contains(e.Id))
                .Select(e =>
                {
                    this.entityIds.Add(e.Id);
                    return e;
                });
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
                    // a known entity isn't notified again
                    if (this.entityIds.Contains(value.Changed.Id))
                        break;
                    // an unknown entity id further inspected)
                    var entity = this.persistence.Entities.FindById(value.Changed.Id);
                    if (entity.Tags.Contains(this.tag))
                        this.Added?.Invoke(entity);
                    else
                        this.Removed?.Invoke(entity.Id);
                    this.entityIds.Add(entity.Id);
                    break;

                case ChangeTypeValues.Removed:
                    // an unknown entity isn't notified
                    if (!this.entityIds.Contains(value.Changed.Id))
                        break;
                    this.Removed?.Invoke(value.Changed.Id);
                    break;
            }
        }

        public Action<Entity> Added { private get; set; }
        public Action<Guid> Removed { private get; set; }

        #endregion Add or remove Entites from result set
    }
}