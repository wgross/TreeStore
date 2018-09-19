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

        private void entityListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditEntityCommand.Execute(this.ViewModel.SelectedEntity);

        private void entityListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var entityViewModel = ((FrameworkElement)sender).DataContext as EntityViewModel;
                if (entityViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(EntityViewModel), entityViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }
    }
}