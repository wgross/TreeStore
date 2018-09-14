using Kosmograph.Desktop.ViewModel;
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
    }
}