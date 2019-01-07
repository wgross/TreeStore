using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel.Base
{
    public abstract class EditModelBase : ViewModelBase
    {
        public EditModelBase()
        {
            this.CommitCommand = new RelayCommand(this.Commit, this.CanCommit);
            this.RollbackCommand = new RelayCommand(this.Rollback);
        }

        public RelayCommand CommitCommand { get; }

        protected virtual bool CanCommit() => true;

        protected abstract void Commit();

        public ICommand RollbackCommand { get; }

        protected abstract void Rollback();
    }
}