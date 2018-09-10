using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditNamedViewModel<T> : INotifyPropertyChanged
        where T : EntityBase
    {
        protected T Edited { get; private set; }

        public EditNamedViewModel(T edited)
        {
            this.Edited = edited;
            this.name = this.Edited.Name;
        }

        public string Name
        {
            get => this.name;
            set => this.RaisePropertyChangedEvent(nameof(Name), ref this.name, value);
        }

        private string name;

        #region INotifyProperyChnaged

        protected void RaisePropertyChangedEvent<V>(string name, ref V field, V value)
        {
            if (EqualityComparer<V>.Default.Equals(field, value))
                return;

            V temp = field;
            field = value;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyProperyChnaged
    }
}