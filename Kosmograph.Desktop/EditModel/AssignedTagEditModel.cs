using GalaSoft.MvvmLight;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
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

        public override void Commit()
        {
            this.Properties.ForEach(p => p.Commit());
        }

        public override void Rollback()
        {
            this.Properties.ForEach(p => p.Rollback());
        }
    }
}