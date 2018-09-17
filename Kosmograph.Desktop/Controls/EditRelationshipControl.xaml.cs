using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditRelationshipControl : UserControl
    {
        public EditRelationshipControl()
        {
            this.InitializeComponent();
        }

        private RelationshipEditModel ViewModel => this.DataContext as RelationshipEditModel;

        private void editEntityControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tag)))
            {
                //sthis.ViewModel.AssignTagCommand.Execute(e.Data.GetData(typeof(Tag)));
            }
            e.Handled = true;
        }
    }
}