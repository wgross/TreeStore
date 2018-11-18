using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.MsaglGraph
{
    public partial class KosmographViewer
    {
        public Canvas GraphCanvas { get; }

        private void GraphCanvasHide() => this.GraphCanvas.InvokeInUiThread(c => c.Visibility = Visibility.Hidden);

        private void GraphCanvasShow() => this.GraphCanvas.InvokeInUiThread(c => c.Visibility = Visibility.Visible);

        private void GraphCanvasClearChildren() => this.GraphCanvas.InvokeInUiThread(c => c.Children.Clear());

        private void GraphCanvasAddChildren(IEnumerable<FrameworkElement> children)
        {
            this.GraphCanvas.InvokeInUiThread(c =>
            {
                foreach (var child in children)
                    c.Children.Add(child);
            });
        }

        private void GraphCanvasRemoveChildren(IEnumerable<FrameworkElement> children)
        {
            this.GraphCanvas.InvokeInUiThread(c =>
            {
                foreach (var child in children)
                    c.Children.Remove(child);
            });
        }
    }
}