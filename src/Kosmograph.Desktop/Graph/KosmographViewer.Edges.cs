using Microsoft.Msagl.Layout.LargeGraphLayout;
using System;
using System.Linq;
using System.Windows.Controls;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private void PrepareEdgeLabels(DrawingEdge drawingEdge, out TextBlock textBlock)
        {
            textBlock = this.CreateTextBlockFromDrawingObjectLabel(drawingEdge.Label);

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingEdge] = textBlock;

            textBlock.Tag = new KosmographViewerEdgeLabel(drawingEdge.Label, textBlock);
        }

        private void CreateEdgeViewers()
        {
            foreach (var edge in this.drawingGraph.Edges)
                this.CreateEdgeViewer(edge, lgSettings: null);
        }

        private KosmographViewerEdge CreateEdgeViewer(DrawingEdge edge, LgLayoutSettings lgSettings)
        {
            lock (this.syncRoot)
            {
                if (this.drawingObjectsToIViewerObjects.TryGetValue(edge, out var existingEdgeViewer))
                    return (KosmographViewerEdge)existingEdgeViewer;

                if (lgSettings != null)
                    return CreateEdgeForLgCase(lgSettings, edge);

                // fetches the textBlock 'Fill' method
                TextBlock labelTextBox;
                this.drawingObjectsToFrameworkElements.TryGetValue(edge, out labelTextBox);
                var edgeViewer = new KosmographViewerEdge(edge, (KosmographViewerEdgeLabel)(labelTextBox.Tag));

                this.drawingObjectsToIViewerObjects[edge] = edgeViewer;

                var zIndex = this.ZIndexOfEdge(edge);
                this.GraphCanvasAddChildren(edgeViewer.FrameworkElements.Select(fe =>
                {
                    Panel.SetZIndex(fe, zIndex);
                    return fe;
                }));

                return edgeViewer;
            }
        }

        private int ZIndexOfEdge(DrawingEdge edge)
        {
            var source = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.SourceNode];
            var target = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.TargetNode];

            var zIndex = Math.Max(source.ZIndex, target.ZIndex) + 1;
            return zIndex;
        }

        private KosmographViewerEdge CreateEdgeForLgCase(LgLayoutSettings lgSettings, DrawingEdge edge)
        {
            return (KosmographViewerEdge)(drawingObjectsToIViewerObjects[edge] = new KosmographViewerEdge(edge, lgSettings)
            {
                PathStrokeThicknessFunc = () => GetBorderPathThickness() * edge.Attr.LineWidth
            });
        }
    }
}