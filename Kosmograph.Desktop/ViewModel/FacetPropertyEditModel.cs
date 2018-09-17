using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class FacetPropertyEditModel : NamedEditModelBase<FacetPropertyViewModel, FacetProperty>
    {
        public FacetPropertyEditModel(FacetPropertyViewModel property)
            : base(property)
        {
        }
    }
}