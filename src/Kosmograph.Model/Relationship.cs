using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Relationship : TaggedItemBase
    {
        public Relationship(string name, Entity from, Entity to, params Tag[] tags)
            : base(name, tags)
        {
            this.From = from;
            this.To = to;
        }

        public Relationship()
            : base(string.Empty, new Tag[0])
        {
        }

        public Entity From { get; set; }

        public Entity To { get; set; }
    }
}