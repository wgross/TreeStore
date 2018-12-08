using System.Reactive.Subjects;

namespace Kosmograph.Messaging
{
    public class EntityMessageBus : ChangedMessageBusBase<IEntity>
    {
        private readonly Subject<EntityChangedMessage> observableSubject = new Subject<EntityChangedMessage>();
    }
}