using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public static class TreeStoreModelExtensions
    {
        #region Retrieve complete data of an entities assigned tags and properties with values

        /// <summary>
        /// From an Entity retrieve all facet properties with values
        /// </summary>
        /// <param name="thisEntity"></param>
        /// <returns></returns>
        public static IEnumerable<(string tagName, string propertyName, bool has, object? value)> AllAssignedPropertyValues(this Entity thisEntity)
            => thisEntity.Tags.SelectMany(t => thisEntity.AllAssignedPropertyValues(t));

        /// <summary>
        ///  from an entity and a tag retrueve all properties with value
        /// </summary>
        /// <param name="thisEntity"></param>
        /// <param name="assignedTag"></param>
        /// <returns></returns>
        public static IEnumerable<(string tagName, string propertyName, bool has, object? value)> AllAssignedPropertyValues(this Entity thisEntity, Tag assignedTag)
            => assignedTag.Facet.Properties.Select(p => thisEntity.AssignedPropertyValue(assignedTag, p));

        /// <summary>
        ///  from an entity and a tag retreuev all properties with value
        /// </summary>
        /// <param name="thisEntity"></param>
        /// <param name="assignedTag"></param>
        /// <returns></returns>
        public static (string tagName, string propertyName, bool has, object? value) AssignedPropertyValue(this Entity thisEntity, Tag assignedTag, FacetProperty facetProperty)
            => thisEntity.TryGetFacetProperty(facetProperty) switch
            {
                (false, _) => (assignedTag.Name, facetProperty.Name, false, default),
                (true, var v) => (assignedTag.Name, facetProperty.Name, true, v)
            };

        #endregion Retrieve complete data of an entities assigned tags and properties with values
    }
}