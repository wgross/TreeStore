using Kosmograph.Desktop.EditModel.Base;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editor
{
    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(EditorCommands.Ok, this.OkExecuted, this.OkCanExecute));
            this.CommandBindings.Add(new CommandBinding(EditorCommands.Cancel, this.CancelExecuted, this.CancelCanExecute));
        }

        public EditModelBase ViewModel => (EditModelBase)this.DataContext;

        private void CancelCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ViewModel.RollbackCommand.CanExecute(e.Parameter);

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.RollbackCommand.Execute(e.Parameter);
        }

        private void OkCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.ViewModel.CommitCommand.CanExecute(e.Parameter);

        private void OkExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.CommitCommand.Execute(e.Parameter);
        }
    }
}