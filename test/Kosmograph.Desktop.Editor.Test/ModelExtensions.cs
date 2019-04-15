using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;

namespace Kosmograph.Desktop.Editor.Test
{
    public static class ModelExtensions
    {
        public static TagViewModel ToViewModel(this Tag model) => new TagViewModel(model);

        public static EntityViewModel ToViewModel(this Entity model) => new EntityViewModel(model);

        public static RelationshipViewModel ToViewModel(this Relationship model) => new RelationshipViewModel(model);

        public static FacetPropertyViewModel ToViewModel(this FacetProperty property) => new FacetPropertyViewModel(property);
    }
}