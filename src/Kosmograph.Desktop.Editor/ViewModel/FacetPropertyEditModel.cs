﻿using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Linq;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public sealed class FacetPropertyEditModel : NamedEditModelBase<FacetProperty>
    {
        public FacetPropertyEditModel(TagEditModel editTag, FacetProperty property)
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
        }

        #endregion Implement Validate

        protected override bool CanCommit()
        {
            return !this.HasErrors && base.CanCommit();
        }
    }
}