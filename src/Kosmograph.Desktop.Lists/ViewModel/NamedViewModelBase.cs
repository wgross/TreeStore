using GalaSoft.MvvmLight;
using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public abstract class NamedViewModelBase<T> : ViewModelBase
        where T : NamedBase
    {
        public NamedViewModelBase(T model)
        {
            this.Model = model;
        }

        public T Model { get; }

        public string Name => this.Model.Name;

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj is NamedViewModelBase<T> vm)
                return this.Model.Equals(vm.Model);

            return false;
        }

        public override int GetHashCode() => (this.GetType(), this.Model).GetHashCode();
    }
}