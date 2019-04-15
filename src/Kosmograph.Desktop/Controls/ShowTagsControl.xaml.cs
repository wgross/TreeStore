using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Controls
{
    public partial class ShowTagsControl : UserControl
    {
        public ShowTagsControl()
        {
            this.InitializeComponent();
        }

        KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        private void tagListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var tagViewModel = ((FrameworkElement)sender).DataContext as Lists.ViewModel.TagViewModel;
                if (tagViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Lists.ViewModel.TagViewModel), tagViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        private void tagRepositoryView_EditTag(object sender, RoutedEventArgs e) => this.ViewModel.EditTagCommand.Execute(this.tagRepositoryView.SelectedItem);

        private void tagRepositoryView_CreateTag(object sender, RoutedEventArgs e) => this.ViewModel.CreateTagCommand.Execute(null);

        private void tagRepositoryView_DeleteTag(object sender, RoutedEventArgs e) => this.ViewModel.DeleteTagCommand.Execute(this.tagRepositoryView.SelectedItem);
    }
}