using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System.Linq;
using System.Windows;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private const double DesiredPathThicknessInInches = 0.008;

        private double GetBorderPathThickness() => DesiredPathThicknessInInches * DpiX;

        private void CreateViewerNodes()
        {
            foreach (var node in this.Graph.Nodes.Concat(this.Graph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf()))
            {
                this.Invalidate(this.GetOrCreateViewerNode(node));
            }
        }

        private IViewerNode GetOrCreateViewerNode(Node drawingNode)
        {
            // this moethod looks like weird twin of IVIewer.CreateIViewerNode...
            lock (this.syncRoot)
            {
                if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingNode, out var existingViewerNode))
                    return (IViewerNode)existingViewerNode;

                FrameworkElement feOfLabel;
                if (!this.drawingObjectsToFrameworkElements.TryGetValue(drawingNode, out feOfLabel))
                    feOfLabel = this.CreateAndRegisterFrameworkElementOfDrawingNode(drawingNode);

                var viewerNode = new KosmographViewerNode(drawingNode, feOfLabel,
                    funcFromDrawingEdgeToVEdge: e => (VEdge)drawingObjectsToIViewerObjects[e],
                    pathStrokeThicknessFunc: () => GetBorderPathThickness() * drawingNode.Attr.LineWidth);

                foreach (var fe in viewerNode.FrameworkElements)
                    this.GraphCanvas.Children.Add(fe);

                // remember the created KosmographViewerNode.
                this.drawingObjectsToIViewerObjects[drawingNode] = viewerNode;

                return viewerNode;
            }
        }
    }
}