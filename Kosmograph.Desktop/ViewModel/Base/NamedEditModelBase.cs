using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.ViewModel
{
    public class NamedEditModelBase<VM, M> : EditModelBase
            where M : NamedItemBase
            where VM : NamedViewModelBase<M>
    {
        public VM ViewModel { get; private set; }

        public NamedEditModelBase(VM edited)
        {
            this.ViewModel = edited;
        }

        public override void Commit()
        {
            this.ViewModel.Name = this.Name;
        }

        public override void Rollback()
        {
            this.Name = this.ViewModel.Name;
        }

        public string Name
        {
            get => this.name ?? this.ViewModel.Name;
            set => this.Set(nameof(Name), ref this.name, value);
        }

        private string name;
    }
}