namespace Kosmograph.Messaging
{
    public class EntityChangedMessage : ChangedMessage<IEntity>
    {
        public EntityChangedMessage(ChangeTypeValues changeType, IEntity changed)
            : base(changeType, changed)
        { }
    }
}