using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
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

        private void entityListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var entityViewModel = ((FrameworkElement)sender).DataContext as Lists.ViewModel.EntityViewModel;
                if (entityViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Entity), entityViewModel.Model);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        private void entityRepositoryView_EditEntity(object sender, RoutedEventArgs e)
            => this.ViewModel.EditEntityCommand.Execute(this.entityRepositoryView.SelectedItem);

        private void entityRepositoryView_CreateEntity(object sender, RoutedEventArgs e)
            => this.ViewModel.CreateEntityCommand.Execute(null);

        private void entitRepositoryView_DeleteEntity(object sender, RoutedEventArgs e)
            => this.ViewModel.DeleteEntityCommand.Execute(this.entityRepositoryView.SelectedItem);
    }
}