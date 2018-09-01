using LiteDB;
using System;

namespace Kosmograph.Model
{
    public abstract class EntityBase
    {
        public ObjectId Id { get; set; }
    }
}
