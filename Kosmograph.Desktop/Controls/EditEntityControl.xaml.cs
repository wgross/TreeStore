using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditEntityControl : UserControl
    {
        public EditEntityControl()
        {
            this.InitializeComponent();
        }

        private EntityEditModel ViewModel => this.DataContext as EntityEditModel;

        private void editEntityControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TagViewModel)))
            {
                this.ViewModel.AssignTagCommand.Execute(e.Data.GetData(typeof(TagViewModel)));
            }
            e.Handled = true;
        }
    }
}