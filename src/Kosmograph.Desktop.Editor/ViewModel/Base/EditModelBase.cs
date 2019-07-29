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

        public bool HasErrors
        {
            get => this.hasErrors;
            protected set => this.Set(nameof(HasErrors), ref this.hasErrors, value);
        }

        private bool hasErrors;

        protected virtual bool CanCommit() => !this.HasErrors;

        protected abstract void Commit();

        public ICommand RollbackCommand { get; }

        protected abstract void Rollback();
    }
}