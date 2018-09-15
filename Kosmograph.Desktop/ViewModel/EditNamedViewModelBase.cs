using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Model.Base;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public abstract class EditKosmographItemViewModelBase : ViewModelBase
    {
        public EditKosmographItemViewModelBase()
        {
            this.CommitCommand = new RelayCommand(this.Commit);
            this.RollbackCommand = new RelayCommand(this.Rollback);
        }

        public ICommand CommitCommand { get; }

        public ICommand RollbackCommand { get; }

        public abstract void Commit();

        public abstract void Rollback();
    }

    public class EditNamedViewModelBase<T> : EditKosmographItemViewModelBase
            where T : NamedItemBase
    {
        public T Model { get; private set; }

        public EditNamedViewModelBase(T edited)
        {
            this.Model = edited;
        }

        public override void Commit()
        {
            this.Model.Name = this.Name;
        }

        public override void Rollback()
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