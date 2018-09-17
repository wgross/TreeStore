using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedTagEditModel : NamedViewModelBase<Tag>
    {
        private readonly Tag tag;

        public AssignedTagEditModel(Tag tag, IDictionary<string, object> propertyValues)
            : base(tag)
        {
            this.Properties = new CommitableObservableCollection<EditAssignedFacetPropertyValueViewModel>(this.Model.Facet.Properties.Select(p => new EditAssignedFacetPropertyValueViewModel(p, propertyValues)));
        }

        public CommitableObservableCollection<EditAssignedFacetPropertyValueViewModel> Properties { get; }
    }
}