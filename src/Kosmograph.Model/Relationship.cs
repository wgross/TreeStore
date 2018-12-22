using Kosmograph.Messaging;
using Kosmograph.Model.Base;

namespace Kosmograph.Model
{
    public class Relationship : TaggedBase, Messaging.IRelationship
    {
        public Relationship(string name, Entity from, Entity to, params Tag[] tags)
            : base(name, tags)
        {
            this.From = from;
            this.To = to;
        }

        public Relationship(string name)
            : this(name, null, null)
        {
        }

        public Relationship()
            : base(string.Empty, new Tag[0])
        {
        }

        public Entity From { get; set; }

        public Entity To { get; set; }

        IEntity IRelationship.From => this.From;

        IEntity IRelationship.To => this.To;
    }
}