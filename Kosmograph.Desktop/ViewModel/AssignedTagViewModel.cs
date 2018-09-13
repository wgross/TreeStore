using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedTagViewModel : NamedViewModelBase<Tag>
    {
        private readonly Tag tag;

        public AssignedTagViewModel(Tag tag, IDictionary<string, object> propertyValues)
            : base(tag)
        {
            this.Properties = new CommitableObservableCollection<AssignedFacetPropertyValue>(this.Model.Facet.Properties.Select(p => new AssignedFacetPropertyValue(p, propertyValues)));
        }

        public CommitableObservableCollection<AssignedFacetPropertyValue> Properties { get; }
    }
}