using System;

namespace Kosmograph.Model.Base
{
    public abstract class NamedBase
    {
        #region Construction and initialization of this instance

        public NamedBase(string name)
        {
            this.Name = name;
        }

        public NamedBase()
            : this(string.Empty)

        {
        }

        #endregion Construction and initialization of this instance

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if(obj is NamedBase nb)
                return (this.GetType(), this.Id).Equals((obj.GetType(), nb.Id));

            return false;            
        }

        public override int GetHashCode() => (this.GetType(), this.Id).GetHashCode();
    }
}