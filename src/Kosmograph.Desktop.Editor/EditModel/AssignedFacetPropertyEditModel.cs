﻿using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;

namespace Kosmograph.Desktop.EditModel
{
    public class AssignedFacetPropertyEditModel : EditModelBase
    {
        public AssignedFacetPropertyEditModel(AssignedFacetPropertyViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public AssignedFacetPropertyViewModel ViewModel { get; }

        public object Value
        {
            get => this.value ?? this.ViewModel.Value;
            set => this.Set(nameof(Value), ref this.value, value);
        }

        private object value = null;

        override protected void Commit()
        {
            this.ViewModel.Value = this.Value;
        }

        override protected void Rollback()
        {
            this.value = null;
        }
    }
}