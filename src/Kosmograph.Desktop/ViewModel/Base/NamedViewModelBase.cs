using GalaSoft.MvvmLight;
using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.ViewModel.Base
{
    public abstract class NamedViewModelBase<T> : ViewModelBase
        where T : NamedItemBase
    {
        public NamedViewModelBase(T model)
        {
            this.Model = model;
        }

        public T Model { get; }

        public string Name
        {
            get => this.Model.Name;
            set
            {
                if (this.Model.Name.Equals(value))
                    return;
                this.Model.Name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }
    }
}