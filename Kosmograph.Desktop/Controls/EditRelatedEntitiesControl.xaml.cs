using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditRelatedEntitiesControl : UserControl
    {
        public EditRelatedEntitiesControl()
        {
            this.InitializeComponent();
        }

        private RelationshipEditModel ViewModel => this.DataContext as RelationshipEditModel;

        private void fromDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(EntityViewModel)))
            {
                this.ViewModel.From = (EntityViewModel)e.Data.GetData(typeof(EntityViewModel));
            }
            e.Handled = true;
        }

        private void toDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(EntityViewModel)))
            {
                this.ViewModel.To = (EntityViewModel)e.Data.GetData(typeof(EntityViewModel));
            }
            e.Handled = true;
        }
    }
}