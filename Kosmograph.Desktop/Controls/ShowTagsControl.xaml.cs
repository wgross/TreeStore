using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
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

        private void tagListBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ViewModel.SelectedTag is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Tag), this.ViewModel.SelectedTag.Model);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Link);
            }
        }

        private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditTagCommand.Execute(this.ViewModel.SelectedTag);
    }
}