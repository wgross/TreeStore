using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.Model
{
    public interface IEntityRepository
    {
        Entity FindById(Guid id);

        IEnumerable<Entity> FindAll();

        Entity Upsert(Entity tag);

        bool Delete(Guid id);
    }
}
