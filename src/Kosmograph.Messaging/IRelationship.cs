namespace Kosmograph.Messaging
{
    public interface IRelationship : IIdentifiable
    {
        IEntity From { get; }
        IEntity To { get; }
    }
}