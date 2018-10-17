using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.EditModel.Base
{
    public abstract class NamedEditModelBase<VM, M> : EditModelBase
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
    }
}