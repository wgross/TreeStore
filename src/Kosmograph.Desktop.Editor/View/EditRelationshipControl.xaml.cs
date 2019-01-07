using Kosmograph.Desktop.Editors.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Editors.View
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
            if (e.Data.GetDataPresent(typeof(TagViewModel)))
            {
                this.ViewModel.AssignTagCommand.Execute(e.Data.GetData(typeof(TagViewModel)));
            }
            e.Handled = true;
        }
    }
}