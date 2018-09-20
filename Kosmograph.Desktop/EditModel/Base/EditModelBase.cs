using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel.Base
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

        public abstract void Commit();

        public ICommand RollbackCommand { get; }

        public abstract void Rollback();
    }
}