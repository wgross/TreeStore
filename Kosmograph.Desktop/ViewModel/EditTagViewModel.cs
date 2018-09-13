using Kosmograph.Model;
using System;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditTagViewModel : EditNamedViewModel<Tag>
    {
        private readonly Action<Tag> committed;

        public EditTagViewModel(Tag tag, Action<Tag> committed)
            : base(tag)
        {
            this.Facet = new EditFacetViewModel(tag.Facet);
            this.committed = committed;
        }

        public EditFacetViewModel Facet { get; }

        public override void Commit()
        {
            this.Facet.Commit();
            base.Commit();
            this.committed(this.Model);
        }

        public override void Rollback()
        {
            this.Facet.Rollback();
            base.Rollback();
        }
    }
}