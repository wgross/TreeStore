using Kosmograph.Messaging;
using System;

namespace Kosmograph.Model
{
    public class ModelController : IDisposable, IObserver<ChangedMessage<ITag>>, IObserver<ChangedMessage<IEntity>>, IObserver<ChangedMessage<IRelationship>>
    {
        private IDisposable? tagSubscription;
        private IDisposable? entitySubscription;
        private IDisposable? relationshipSubscription;

        public ModelController(KosmographModel model)
            : this(model.MessageBus)
        {
            this.Model = model;
        }

        public KosmographModel Model { get; }

        public ModelController(IKosmographMessageBus kosmographMessageBus)
            : this(kosmographMessageBus.Tags, kosmographMessageBus.Entities, kosmographMessageBus.Relationships)
        { }

        private ModelController(IChangedMessageBus<ITag> tags, IChangedMessageBus<IEntity> entities, IChangedMessageBus<IRelationship> relationships)
        {
            this.tagSubscription = tags.Subscribe(this);
            this.entitySubscription = entities.Subscribe(this);
            this.relationshipSubscription = relationships.Subscribe(this);
        }

        public void Dispose()
        {
            this.tagSubscription?.Dispose();
            this.tagSubscription = null;
            this.entitySubscription?.Dispose();
            this.entitySubscription = null;
            this.relationshipSubscription?.Dispose();
            this.relationshipSubscription = null;
        }

        #region Publish model changes

        public Action<Tag> TagChanged { private get; set; }

        public Action<Guid> TagRemoved { private get; set; }

        public Action<Entity> EntityChanged { private get; set; }

        public Action<Guid> EntityRemoved { private get; set; }

        public Action<Relationship> RelationshipChanged { private get; set; }

        public Action<Guid> RelationshipRemoved { private get; set; }

        #endregion Publish model changes

        #region Observe Tags

        void IObserver<ChangedMessage<ITag>>.OnNext(ChangedMessage<ITag> value)
        {
            if (value.ChangeType.Equals(ChangeTypeValues.Modified))
                this.OnChangingTag((Tag)value.Changed);
            else
                this.OnRemovingTag(value.Changed.Id);
        }

        virtual protected void OnChangingTag(Tag tag) => this.TagChanged?.Invoke(tag);

        virtual protected void OnRemovingTag(Guid tagId) => this.TagRemoved?.Invoke(tagId);

        void IObserver<ChangedMessage<ITag>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<ITag>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        #endregion Observe Tags

        #region Observe Entities

        void IObserver<ChangedMessage<IEntity>>.OnNext(ChangedMessage<IEntity> value)
        {
            if (value.ChangeType.Equals(ChangeTypeValues.Modified))
                this.OnEntityChanging((Entity)value.Changed);
            else
                this.OnEntityRemoving(value.Changed.Id);
        }

        virtual protected void OnEntityRemoving(Guid entityId) => this.EntityRemoved?.Invoke(entityId);

        virtual protected void OnEntityChanging(Entity changed) => this.EntityChanged?.Invoke(changed);

        void IObserver<ChangedMessage<IEntity>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IEntity>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        #endregion Observe Entities

        #region Observe Relationships

        void IObserver<ChangedMessage<IRelationship>>.OnNext(ChangedMessage<IRelationship> value)
        {
            if (value.ChangeType.Equals(ChangeTypeValues.Modified))
                this.OnRelationshipChanging((Relationship)value.Changed);
            else
                OnRelationshipRemoving(value.Changed.Id);
        }

        virtual protected void OnRelationshipRemoving(Guid relationshipId) => this.RelationshipRemoved?.Invoke(relationshipId);

        virtual protected void OnRelationshipChanging(Relationship changed) => this.RelationshipChanged?.Invoke(changed);

        void IObserver<ChangedMessage<IRelationship>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IRelationship>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        #endregion Observe Relationships
    }
}