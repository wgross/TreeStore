using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class AssignedTagEditModel : EditModelBase
    {
        public AssignedTagEditModel(Tag tagModel, IDictionary<string, object> values)
        {
            this.Model = tagModel;
            this.Properties = new CommitableObservableCollection<AssignedFacetPropertyEditModel>(
                tagModel.Facet.Properties.Select(p => new AssignedFacetPropertyEditModel(p, values)));
        }

        public Tag Model { get; }

        public CommitableObservableCollection<AssignedFacetPropertyEditModel> Properties { get; }

        protected override bool CanCommit() => this.Properties.Aggregate(true, (ok, p) => !p.HasErrors && ok);

        protected override void Commit() => this.Properties.ForEach(p => p.CommitCommand.Execute(null));

        protected override void Rollback() => this.Properties.ForEach(p => p.RollbackCommand.Execute(null));
    }
}