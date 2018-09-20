using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public static class ModelExtensions
    {
        public static TagViewModel ToViewModel(this Tag model) => new TagViewModel(model);

        public static EntityViewModel ToViewModel(this Entity model) => new EntityViewModel(model);

        public static FacetPropertyViewModel ToViewModel(this FacetProperty property) => new FacetPropertyViewModel(property);
    }
}