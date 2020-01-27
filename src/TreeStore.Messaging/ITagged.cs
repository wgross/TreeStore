using System.Collections.Generic;

namespace TreeStore.Messaging
{
    public interface ITagged
    {
        IEnumerable<ITag> Tags { get; }
    }
}