using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for ShowTagsControl.xaml
    /// </summary>
    public partial class ShowTagsControl : UserControl
    {
        public ShowTagsControl()
        {
            this.InitializeComponent();
        }

        KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        //private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        //    this.ViewModel.Tags.EditCommand.Execute(this.tagListBox.SelectedItem);

        private void tagListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var tagViewModel = ((FrameworkElement)sender).DataContext as TagViewModel;
                if (tagViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(TagViewModel), tagViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        private void tagRepositoryView_EditTag(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Tags.EditCommand.Execute(this.tagRepositoryView.SelectedItem);
        }
    }
}