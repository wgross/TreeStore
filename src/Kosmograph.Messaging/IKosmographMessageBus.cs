using System;

namespace Kosmograph.Messaging
{
    public interface IKosmographMessageBus
    {
        IChangedMessageBus<IEntity> Entities { get; }

        IChangedMessageBus<IRelationship> Relationships { get; }
    }

    public interface IChangedMessageBus<T> : IObservable<ChangedMessage<T>>
    {
        void Added(T added);

        void Modified(T modified);

        void Removed(T removed);
    }
}