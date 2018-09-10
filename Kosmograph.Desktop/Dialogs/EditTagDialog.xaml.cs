using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTagDialog.xaml
    /// </summary>
    public partial class EditTagDialog : Window
    {
        public EditTagDialog()
        {
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(DialogCommands.Ok, this.OkExecuted, this.OkCanExecute));
            this.CommandBindings.Add(new CommandBinding(DialogCommands.Cancel, this.CancelExecuted, this.CancelCanExecute));
        }

        private void CancelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CancelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OkCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OkExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}