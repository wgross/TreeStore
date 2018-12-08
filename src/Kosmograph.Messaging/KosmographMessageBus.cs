namespace Kosmograph.Messaging
{
    public class KosmographMessageBus : IKosmographMessageBus
    {
        public IChangedMessageBus<IEntity> Entities { get; } = new EntityMessageBus();

        public IChangedMessageBus<IRelationship> Relationships { get; } = new RelationshipMessageBus();
    }
}