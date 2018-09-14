using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDialog.xaml
    /// </summary>
    public partial class EditDialog : Window
    {
        public EditDialog()
        {
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(DialogCommands.Ok, this.OkExecuted, this.OkCanExecute));
            this.CommandBindings.Add(new CommandBinding(DialogCommands.Cancel, this.CancelExecuted, this.CancelCanExecute));
        }

        public EditKosmographItemViewModelBase ViewModel => (EditKosmographItemViewModelBase)this.DataContext;

        private void CancelCanExecute(object sender, CanExecuteRoutedEventArgs e) => this.ViewModel.RollbackCommand.CanExecute(e.Parameter);

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.RollbackCommand.Execute(e.Parameter);
            this.Close();
        }

        private void OkCanExecute(object sender, CanExecuteRoutedEventArgs e) => this.ViewModel.CommitCommand.CanExecute(e.Parameter);

        private void OkExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.CommitCommand.Execute(e.Parameter);
            this.Close();
        }
    }
}