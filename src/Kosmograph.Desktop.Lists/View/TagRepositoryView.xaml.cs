using Kosmograph.Desktop.Lists.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Lists.View
{
    public partial class TagRepositoryView : UserControl
    {
        public TagRepositoryView()
        {
            this.InitializeComponent();
        }

        TagRepositoryViewModel ViewModel => this.DataContext as TagRepositoryViewModel;

        //private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        //    this.ViewModel.Tags.EditCommand.Execute(this.tagListBox.SelectedItem);

        private void repositoryListBoxItem_MouseMove(object sender, MouseEventArgs e)
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
    }
}