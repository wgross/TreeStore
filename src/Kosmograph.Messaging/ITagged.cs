using System.Collections.Generic;

namespace Kosmograph.Messaging
{
    public interface ITagged
    {
        IEnumerable<ITag> Tags { get; }
    }
}