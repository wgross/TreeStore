using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditTagViewModel : EditNamedViewModel<Tag>
    {
        public EditTagViewModel(Tag tag)
            : base(tag)
        {
            this.Facet = new EditFacetViewModel(tag.Facet);
        }

        public EditFacetViewModel Facet { get; }
    }
}