using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using System.Linq;

namespace Kosmograph.Desktop.EditModel
{
    public class AssignedTagEditModel : EditModelBase
    {
        public AssignedTagEditModel(AssignedTagViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Properties = new CommitableObservableCollection<AssignedFacetPropertyEditModel>(
                viewModel.Properties.Select(p => new AssignedFacetPropertyEditModel(p)));
        }

        public AssignedTagViewModel ViewModel { get; }

        public CommitableObservableCollection<AssignedFacetPropertyEditModel> Properties { get; }

        protected override void Commit()
        {
            this.Properties.ForEach(p => p.CommitCommand.Execute(null));
        }

        protected override void Rollback()
        {
            this.Properties.ForEach(p => p.RollbackCommand.Execute(null));
        }
    }
}