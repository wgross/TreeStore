namespace Kosmograph.Messaging
{
    public enum ChangeTypeValues
    {
        Added,
        Removed,
        Modified
    }

    public class ChangedMessage<T>
    {
        public ChangedMessage(ChangeTypeValues changeType, T changed)
        {
            this.ChangeType = changeType;
            this.Changed = changed;
        }

        public ChangeTypeValues ChangeType { get; }

        public T Changed { get; }
    }
}