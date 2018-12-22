using System.Linq;

namespace Kosmograph.Model
{
    public static class KosomographPersistenceExtensions
    {
        public static bool RemoveWithRelationship(this IKosmographPersistence thisPersistence, Entity entity)
        {
            foreach (var affectedRelationship in thisPersistence.Relationships.FindByEntity(entity).ToArray())
                thisPersistence.Relationships.Delete(affectedRelationship);

            return thisPersistence.Entities.Delete(entity);
        }
    }
}