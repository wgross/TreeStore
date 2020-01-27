using System.Linq;

namespace TreeStore.Model
{
    public static class KosmographPersistenceExtensions
    {
        public static bool RemoveWithRelationship(this ITreeStorePersistence thisPersistence, Entity entity)
        {
            foreach (var affectedRelationship in thisPersistence.Relationships.FindByEntity(entity).ToArray())
                thisPersistence.Relationships.Delete(affectedRelationship);

            return thisPersistence.Entities.Delete(entity);
        }
    }
}