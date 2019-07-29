using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Collections.Generic;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class AssignedFacetPropertyEditModel : EditModelBase
    {
        public AssignedFacetPropertyEditModel(FacetProperty model, IDictionary<string, object> values)
        {
            this.Model = model;
            this.Values = values;
        }

        public FacetProperty Model { get; }
        public IDictionary<string, object> Values { get; }

        public object Value
        {
            get => this.value ?? this.ResolvePropertyValue();
            set
            {
                if (this.Set(nameof(Value), ref this.value, value))
                    this.Validate();
            }
        }

        private object ResolvePropertyValue()
        {
            if (this.Values.TryGetValue(this.Model.Id.ToString(), out var resolvedValue))
                return resolvedValue;
            return null;
        }

        private object value = null;

        override protected void Commit()
        {
            if (this.value is null)
                return; // value of null isnt committable!

            this.Values[this.Model.Id.ToString()] = this.value;
        }

        override protected void Rollback()
        {
            this.value = null;
        }

        protected override bool CanCommit()
        {
            this.Validate();
            return (!this.HasErrors && base.CanCommit());
        }

        #region Validate data and indicate error

        public string ValueError
        {
            get => this.valueError;
            protected set => this.Set(nameof(ValueError), ref this.valueError, value?.Trim());
        }

        private string valueError;

        public void Validate()
        {
            this.HasErrors = false;
            if (!this.Model.CanAssignValue(this.Value?.ToString()))
                this.ValueError = $"Value must be of type '{this.Model.Type.ToString()}'";
            else
                this.ValueError = null;
            this.HasErrors = !string.IsNullOrEmpty(this.ValueError);
        }

        #endregion Validate data and indicate error
    }
}