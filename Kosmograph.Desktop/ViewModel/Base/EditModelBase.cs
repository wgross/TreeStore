using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel.Base
{
    public abstract class EditModelBase : ViewModelBase
    {
        public EditModelBase()
        {
            this.CommitCommand = new RelayCommand(this.Commit);
            this.RollbackCommand = new RelayCommand(this.Rollback);
        }

        public ICommand CommitCommand { get; }

        public ICommand RollbackCommand { get; }

        public abstract void Commit();

        public abstract void Rollback();
    }
}