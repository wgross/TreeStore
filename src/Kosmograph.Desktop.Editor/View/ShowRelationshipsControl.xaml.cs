using Kosmograph.Desktop.Editors.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.View
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

        private void relationshipsListBox_MouseMove(object sender, MouseEventArgs e)
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

        private void relationshipsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.Relationships.EditCommand.Execute(this.relationshipsListBox.SelectedItem);
    }
}