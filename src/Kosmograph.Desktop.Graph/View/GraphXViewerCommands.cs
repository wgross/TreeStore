using System.Windows.Input;

namespace Kosmograph.Desktop.Graph.View
{
    public static class GraphXViewerCommands
    {
        public static ICommand ClearCommand = new RoutedUICommand("Clear", "Clear", typeof(GraphXViewer));

        public static ICommand EditTagCommand = new RoutedUICommand("EditTag", "Edit Tag", typeof(GraphXViewerHeaderArea));
    }
}