using GalaSoft.MvvmLight;
using Kosmograph.Model.Base;
using System.ComponentModel;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditNamedViewModel<T> : ViewModelBase, INotifyPropertyChanged
        where T : EntityBase
    {
        public T Model { get; private set; }

        public EditNamedViewModel(T edited)
        {
            this.Model = edited;
            this.name = this.Model.Name;
        }

        public virtual void Commit()
        {
            this.Model.Name = this.name;
        }

        public string Name
        {
            get => this.name;
            set => this.Set(nameof(Name), ref this.name, value);
        }

        private string name;
    }
}