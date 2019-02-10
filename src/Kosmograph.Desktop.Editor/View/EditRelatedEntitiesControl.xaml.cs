using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Editors.View
{
    public partial class EditRelatedEntitiesControl : UserControl
    {
        private bool toDropAreaMouseDown;
        private bool fromDropAreaMouseDown;

        public EditRelatedEntitiesControl()
        {
            this.InitializeComponent();
        }

        private RelationshipEditModel ViewModel => this.DataContext as RelationshipEditModel;

        private void fromDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Entity)))
            {
                var oldfrom = this.ViewModel.From;
                this.ViewModel.From = (Entity)e.Data.GetData(typeof(Entity));
                if (this.toDropAreaMouseDown)
                    this.ViewModel.To = oldfrom;

                this.fromDropAreaMouseDown = this.toDropAreaMouseDown = false;
            }
            e.Handled = true;
        }

        private void toDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Entity)))
            {
                var oldTo = this.ViewModel.To;
                this.ViewModel.To = (Entity)e.Data.GetData(typeof(Entity));
                if (this.fromDropAreaMouseDown)
                    this.ViewModel.From = oldTo;

                this.fromDropAreaMouseDown = this.toDropAreaMouseDown = false;
            }
            e.Handled = true;
        }

        private void toDropArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                this.fromDropAreaMouseDown = this.toDropAreaMouseDown = false;
            }
            else
            {
                var relatsionhipEditModel = ((FrameworkElement)sender).DataContext as RelationshipEditModel;
                if (relatsionhipEditModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Entity), relatsionhipEditModel.To);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        private void fromDropArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                this.fromDropAreaMouseDown = this.toDropAreaMouseDown = false;
            }
            else
            {
                var relatsionhipEditModel = ((FrameworkElement)sender).DataContext as RelationshipEditModel;
                if (relatsionhipEditModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(EntityViewModel), relatsionhipEditModel.From);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        private void toDropArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.toDropAreaMouseDown = true;
        }

        private void fromDropArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.fromDropAreaMouseDown = true;
        }
    }
}