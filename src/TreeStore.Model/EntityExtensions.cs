using System.Linq;

namespace TreeStore.Model
{
    public static class EntityExtensions
    {
        public static void SetFacetProperty<T>(this Entity thisEntity, string tagName, string propertyName, T value) => thisEntity
            .SetFacetProperty<T>(thisEntity
                .Tags
                .Single(t => t.Name.Equals(tagName))
                .Facet
                .Properties
                .Single(p => p.Name.Equals(propertyName)),
            value);
    }
}