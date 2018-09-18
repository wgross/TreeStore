using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class AssignedFacetPropertyEditModel : NamedEditModelBase<AssignedFacetPropertyViewModel, FacetProperty>
    {
        public AssignedFacetPropertyEditModel(AssignedFacetPropertyViewModel viewModel)
            : base(viewModel)
        {
        }

        public object Value
        {
            get => this.value ?? this.ViewModel.Value;
            set => this.Set(nameof(Value), ref this.value, value);
        }

        private object value = null;

        override public void Commit()
        {
            this.ViewModel.Value = this.Value;
        }

        override public void Rollback()
        {
            this.value = null;
        }
    }
}