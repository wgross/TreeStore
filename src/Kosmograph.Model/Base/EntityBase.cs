using System;

namespace Kosmograph.Model
{
    public abstract class EntityBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            var objAsEntityBase = obj as EntityBase;
            if (objAsEntityBase is null)
                return false;

            return (this.GetType(), this.Id).Equals((obj.GetType(), objAsEntityBase.Id));
        }

        public override int GetHashCode() => (this.GetType(), this.Id).GetHashCode();
    }
}