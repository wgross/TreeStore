using System;

namespace TreeStore.Messaging
{
    public interface ITreeStoreMessageBus
    {
        IChangedMessageBus<ITag> Tags { get; }

        IChangedMessageBus<IEntity> Entities { get; }

        IChangedMessageBus<IRelationship> Relationships { get; }
    }

    public interface IChangedMessageBus<T> : IObservable<ChangedMessage<T>>
    {
        void Modified(T modified);

        void Removed(T removed);
    }
}