using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Controls
{
    public partial class ShowEntitiesControl : UserControl
    {
        public ShowEntitiesControl()
        {
            this.InitializeComponent();

        }

        private KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        private void entityListBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ViewModel.SelectedEntity is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(EntityViewModel), this.ViewModel.SelectedEntity);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Link);
            }
        }

        private void entityListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditEntityCommand.Execute(this.ViewModel.SelectedEntity);
    }
}