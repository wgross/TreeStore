using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for ShowTagsControl.xaml
    /// </summary>
    public partial class ShowRelationshipsControl : UserControl
    {
        public ShowRelationshipsControl()
        {
            this.InitializeComponent();
        }

        KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        private void relationshipsListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    if (this.ViewModel.SelectedTag is null)
            //        return;

            //    DataObject data = new DataObject();
            //    data.SetData(typeof(Tag), this.ViewModel.SelectedTag.Model);

            //    DragDrop.DoDragDrop(this, data, DragDropEffects.Link);
            //}
        }

        private void relationshipRepositoryView_EditRelationhip(object sender, RoutedEventArgs e)
            => this.ViewModel.EditRelationshipByIdCommand.Execute(this.relationshipRepositoryView.SelectedItem.Model.Id);

        private void relationshipRepositoryView_CreateRelationship(object sender, RoutedEventArgs e)
            => this.ViewModel.CreateRelationshipCommand.Execute(null);

        private void relationshipRepositoryView_DeleteRelationship(object sender, RoutedEventArgs e)
            => this.ViewModel.DeleteRelationshipByIdCommand.Execute(this.relationshipRepositoryView.SelectedItem.Model.Id);
    }
}