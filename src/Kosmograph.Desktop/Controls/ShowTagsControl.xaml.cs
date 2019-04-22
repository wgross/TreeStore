using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class ShowTagsControl : UserControl
    {
        public ShowTagsControl()
        {
            this.InitializeComponent();
        }

        private KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        private void tagRepositoryView_EditTag(object sender, RoutedEventArgs e)
            => this.ViewModel.EditTagCommand.Execute(this.tagRepositoryView.SelectedItem);

        private void tagRepositoryView_CreateTag(object sender, RoutedEventArgs e)
            => this.ViewModel.CreateTagCommand.Execute(null);

        private void tagRepositoryView_DeleteTag(object sender, RoutedEventArgs e)
            => this.ViewModel.DeleteTagCommand.Execute(this.tagRepositoryView.SelectedItem);
    }
}