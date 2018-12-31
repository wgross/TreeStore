using Kosmograph.Desktop.Lists.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Lists.View
{
    public partial class RelationshipRepositoryView : UserControl
    {
        public RelationshipRepositoryView()
        {
            this.InitializeComponent();
        }

        RelationshipRepositoryViewModel ViewModel => this.DataContext as RelationshipRepositoryViewModel;

        //private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        //    this.ViewModel.Tags.EditCommand.Execute(this.tagListBox.SelectedItem);

        private void repositoryListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var relationshipViewModel = ((FrameworkElement)sender).DataContext as TagViewModel;
                if (relationshipViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(RelationshipViewModel), relationshipViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }
    }
}