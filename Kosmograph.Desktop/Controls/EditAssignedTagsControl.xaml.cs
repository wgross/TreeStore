using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditAssignedTagsControl : UserControl
    {
        public EditAssignedTagsControl()
        {
            this.InitializeComponent();
        }

        private EditEntityViewModel ViewModel => this.DataContext as EditEntityViewModel;

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tag)))
            {
                this.ViewModel.AssignTagCommand.Execute(e.Data.GetData(typeof(Tag)));
            }
            e.Handled = true;
        }
    }
}