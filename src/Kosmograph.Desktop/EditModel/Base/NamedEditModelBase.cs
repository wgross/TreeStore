﻿using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model.Base;
using System;
using System.Collections;
using System.ComponentModel;

namespace Kosmograph.Desktop.EditModel.Base
{
    public abstract class NamedEditModelBase<VM, M> : EditModelBase, INotifyDataErrorInfo
            where M : NamedBase
            where VM : NamedViewModelBase<M>
    {
        public VM ViewModel { get; private set; }

        public NamedEditModelBase(VM edited)
        {
            this.ViewModel = edited;
        }

        protected override void Commit()
        {
            this.ViewModel.Name = this.Name;
        }

        protected override void Rollback()
        {
            this.Name = this.ViewModel.Name;
        }

        public string Name
        {
            get => this.name ?? this.ViewModel.Name;
            set
            {
                if (this.Set(nameof(Name), ref this.name, value?.Trim()))
                    this.Validate();
            }
        }

        private string name;

        public string NameError
        {
            get => this.nameError;
            protected set => this.Set(nameof(NameError), ref this.nameError, value?.Trim());
        }

        private string nameError;

        protected abstract void Validate();

        #region INotifyDataErrorInfo Members

        public bool HasErrors { get; protected set; }

        protected void RaiseErrorsChanged(string name) => this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.Name)));

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public abstract IEnumerable GetErrors(string propertyName);

        #endregion INotifyDataErrorInfo Members

    }
}