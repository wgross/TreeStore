namespace Kosmograph.Messaging
{
    public interface IRelationship : ITagged
    {
        IEntity From { get; }
        IEntity To { get; }
    }
}