using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;

namespace Kosmograph.Desktop.EditModel
{
    public class FacetPropertyEditModel : NamedEditModelBase<FacetPropertyViewModel, FacetProperty>
    {
        public FacetPropertyEditModel(FacetPropertyViewModel property)
            : base(property)
        {
        }
    }
}