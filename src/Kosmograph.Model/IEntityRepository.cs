using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.Model
{
    public interface IEntityRepository : IRepository<Entity>
    {
        IEnumerable<Entity> FindByTag(Tag tag);
    }
}
