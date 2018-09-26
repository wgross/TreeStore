using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private const double DesiredPathThicknessInInches = 0.008;

        private double GetBorderPathThickness() => DesiredPathThicknessInInches * DpiX;

        private void GetOrCreateViewNodes()
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

                FrameworkElement nodeLabel;
                if (!this.drawingObjectsToFrameworkElements.TryGetValue(drawingNode, out nodeLabel))
                    nodeLabel = this.CreateAndRegisterFrameworkElementOfDrawingNode(drawingNode);

                var viewerNode = new KosmographViewerNode(drawingNode, (TextBlock)nodeLabel,
                    funcFromDrawingEdgeToVEdge: e => (VEdge)drawingObjectsToIViewerObjects[e],
                    pathStrokeThicknessFunc: () => GetBorderPathThickness() * drawingNode.Attr.LineWidth);

                this.GraphCanvasAddChildren(viewerNode.FrameworkElements);

                // remember the created KosmographViewerNode.
                this.drawingObjectsToIViewerObjects[drawingNode] = viewerNode;

                return viewerNode;
            }
        }

        private FrameworkElement CreateAndRegisterFrameworkElementOfDrawingNode(Microsoft.Msagl.Drawing.Node node)
        {
            return this.drawingObjectsToFrameworkElements[node] = CreateTextBlockFromDrawingObjectLabel(node.Label);
        }
    }
}