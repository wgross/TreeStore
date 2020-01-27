using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface IRelationshipRepository : IRepository<Relationship>
    {
        IEnumerable<Relationship> FindByEntity(Entity entity);

        IEnumerable<Relationship> FindByTag(Tag tag);

        void Delete(IEnumerable<Relationship> relationships);
    }
}