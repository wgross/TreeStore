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
        }

        public virtual void Commit()
        {
            this.Model.Name = this.Name;
        }

        public virtual void Rollback()
        {
            this.Name = this.Model.Name; 
        }

        public string Name
        {
            get => this.name ?? this.Model.Name;
            set => this.Set(nameof(Name), ref this.name, value);
        }

        private string name;
    }
}