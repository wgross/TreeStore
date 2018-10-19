using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Kosmograph.Desktop.EditModel
{
    public class FacetPropertyEditModel : NamedEditModelBase<FacetPropertyViewModel, FacetProperty>, INotifyDataErrorInfo
    {
        public FacetPropertyEditModel(TagEditModel editTag, FacetPropertyViewModel property)
            : base(property)
        {
            this.Tag = editTag;
        }

        public TagEditModel Tag { get; }

        #region INotifyDataErrorInfo members

        public bool HasErrors => !string.IsNullOrEmpty(this.NameError);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (nameof(Name).Equals(propertyName))
            {
                return this.NameError.Yield();
            }
            return Enumerable.Empty<string>();
        }

        #endregion INotifyDataErrorInfo members

        #region Implement Validate

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.NameError = "Property name must not be empty";
            }
            else if (this.Tag.Properties.Where(p => p.Name.Equals(this.Name)).Count() > 1)
            {
                this.NameError = "Property name must be unique";
            }
            else
            {
                this.NameError = null;
            }
            if (!string.IsNullOrEmpty(this.NameError))
                this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.Name)));
        }
        
        #endregion Implement Validate

        protected override bool CanCommit()
        {
            return !this.HasErrors && base.CanCommit();
        }
    }
}