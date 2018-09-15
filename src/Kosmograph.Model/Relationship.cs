using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Relationship : TaggedItemBase
    {
        public Relationship(string name, params Tag[] tags)
            : base(name, tags)
        {
        }

        public Relationship()
            : base(string.Empty, new Tag[0])
        {
        }
    }
}