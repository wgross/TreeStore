using Kosmograph.Model;
using System;

namespace Kosmograph.Desktop.ViewModel
{
    public class TagEditModel : EditNamedViewModelBase<Tag>
    {
        private readonly Action<Tag> committed;
        private readonly Action<Tag> rolledback;

        public TagEditModel(Tag tag, Action<Tag> committed = null, Action<Tag> rolledback = null)
            : base(tag)
        {
            this.Facet = new EditFacetViewModel(tag.Facet);
            this.committed = committed ?? delegate { };
            this.rolledback = rolledback ?? delegate { };
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
            this.rolledback(this.Model);
        }
    }
}