using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections;
using System.Linq;

namespace Kosmograph.Desktop.EditModel
{
    public sealed class FacetPropertyEditModel : NamedEditModelBase<FacetPropertyViewModel, FacetProperty>
    {
        public FacetPropertyEditModel(TagEditModel editTag, FacetPropertyViewModel property)
            : base(property)
        {
            this.Tag = editTag;
        }

        public TagEditModel Tag { get; }

        #region Implement Validate

        protected override void Validate()
        {
            this.HasErrors = false;
            if (string.IsNullOrEmpty(this.Name))
            {
                this.NameError = "Property name must not be empty";
                this.HasErrors = true;
            }
            else if (this.Tag.Properties.Where(p => p.Name.Equals(this.Name)).Count() > 1)
            {
                this.NameError = "Property name must be unique";
                this.HasErrors = true;
            }
            else
            {
                this.NameError = null;
            }
            if (!string.IsNullOrEmpty(this.NameError))
                this.RaiseErrorsChanged(nameof(this.Name));
        }

        #endregion Implement Validate

        protected override bool CanCommit()
        {
            return !this.HasErrors && base.CanCommit();
        }

        public override IEnumerable GetErrors(string propertyName)
        {
            if (nameof(Name).Equals(propertyName))
            {
                return this.NameError.Yield();
            }
            return Enumerable.Empty<string>();
        }
    }
}