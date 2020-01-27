namespace TreeStore.Messaging
{
    public class TreeStoreMessageBus : ITreeStoreMessageBus
    {
        public static ITreeStoreMessageBus Default { get; set; } = new TreeStoreMessageBus();

        public IChangedMessageBus<ITag> Tags { get; } = new TagMessageBus();

        public IChangedMessageBus<IEntity> Entities { get; } = new EntityMessageBus();

        public IChangedMessageBus<IRelationship> Relationships { get; } = new RelationshipMessageBus();
    }
}