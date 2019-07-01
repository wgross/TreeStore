using Kosmograph.Desktop.Graph.View;
using Kosmograph.Desktop.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for ShowGraphControl.xaml
    /// </summary>
    public partial class ShowGraphControl : UserControl
    {
        public ShowGraphControl()
        {
            InitializeComponent();
        }

        private KosmographViewModel ViewModel => this.DataContext as KosmographViewModel;

        private void graphViewer_EditEntity(object sender, RoutedEventArgs e)
        {
            this.ViewModel.EditEntityByIdCommand.Execute(((EditEntityByIdRoutedEventArgs)e).EntityId);
        }

        private void graphViewer_EditRelationship(object sender, RoutedEventArgs e)
        {
            this.ViewModel.EditEntityByIdCommand.Execute(((EditEntityByIdRoutedEventArgs)e).EntityId);
        }
    }
}