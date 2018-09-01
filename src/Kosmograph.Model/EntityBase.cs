using LiteDB;
using System;

namespace Kosmograph.Model
{
    public abstract class EntityBase
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
