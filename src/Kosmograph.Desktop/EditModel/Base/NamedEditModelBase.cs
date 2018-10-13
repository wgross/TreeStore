using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model.Base;
using System.Runtime.CompilerServices;

namespace Kosmograph.Desktop.EditModel.Base
{
    public abstract class NamedEditModelBase<VM, M> : EditModelBase
            where M : NamedItemBase
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
            set => this.Set(nameof(Name), ref this.name, value.Trim());
        }

        private string name;

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.Validate();
            base.RaisePropertyChanged(propertyName);
        }

        protected abstract void Validate();
    }
}