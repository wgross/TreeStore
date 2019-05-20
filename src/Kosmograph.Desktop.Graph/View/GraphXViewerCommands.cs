using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph.View
{
    public static class GraphXViewerCommands
    {
        public static ICommand ClearCommand = new RoutedUICommand("Clear", "Clear", typeof(GraphXViewer));
    }
}
