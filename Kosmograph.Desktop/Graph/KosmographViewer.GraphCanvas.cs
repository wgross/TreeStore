using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        public Canvas GraphCanvas { get; }

        private void GraphCanvasHide() => this.GraphCanvas.InvokeInUiThread(c => c.Visibility = Visibility.Hidden);

        private void GraphCanvasClearChildren() => this.GraphCanvas.InvokeInUiThread(c => c.Children.Clear());
    }
}