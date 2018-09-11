using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditNamedViewModel<T> : INotifyPropertyChanged
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