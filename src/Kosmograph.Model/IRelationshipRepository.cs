using System.Collections.Generic;

namespace Kosmograph.Model
{
    public interface IRelationshipRepository : IRepository<Relationship>
    {
        IEnumerable<Relationship> FindByEntity(Entity entity);

        void Delete(IEnumerable<Relationship> relationships);
    }
}