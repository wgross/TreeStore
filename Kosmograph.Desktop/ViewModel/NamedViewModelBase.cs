using GalaSoft.MvvmLight;
using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.ViewModel
{
    public abstract class NamedViewModelBase<T> : ViewModelBase
        where T : EntityBase
    {
        public NamedViewModelBase(T model)
        {
            this.Model = model;
        }

        public T Model { get; }

        public string Name => this.Model.Name;
    }
}