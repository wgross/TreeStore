namespace Kosmograph.Messaging
{
    public class KosmographMessageBus : IKosmographMessageBus
    {
        public static IKosmographMessageBus Default { get; set; } = new KosmographMessageBus();

        public IChangedMessageBus<ITag> Tags { get; } = new TagMessageBus();

        public IChangedMessageBus<IEntity> Entities { get; } = new EntityMessageBus();

        public IChangedMessageBus<IRelationship> Relationships { get; } = new RelationshipMessageBus();
    }
}