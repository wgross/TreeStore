using Kosmograph.Model;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedTagEditModel : NamedEditModelBase<AssignedTagViewModel, Tag>
    {
        public AssignedTagEditModel(AssignedTagViewModel viewModel)
            : base(viewModel)
        {
            this.Properties = new CommitableObservableCollection<AssignedFacetPropertyEditModel>(
                viewModel.Properties.Select(p => new AssignedFacetPropertyEditModel(p)));
        }

        public CommitableObservableCollection<AssignedFacetPropertyEditModel> Properties { get; }
    }
}