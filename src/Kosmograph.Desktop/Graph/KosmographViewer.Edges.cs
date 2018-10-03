using Kosmograph.Desktop.ViewModel;
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
            textBlock = VisualsFactory.CreateLabel(drawingEdge.Label);

            this.drawingObjectsToFrameworkElements[drawingEdge] = textBlock;

            textBlock.Tag = new KosmographViewerEdgeLabel(drawingEdge.Label, textBlock);
        }

        private void GetOrCreateEdgeViewers()
        {
            foreach (var edge in this.drawingGraph.Edges)
                this.Invalidate(this.GetOrCreateEdgeViewer(edge, lgSettings: null));
        }

        private KosmographViewerEdge GetOrCreateEdgeViewer(DrawingEdge edge, LgLayoutSettings lgSettings)
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

        public void UpdateEdge(RelationshipViewModel relationship)
        {
            var drawingNodeSource = this.Graph.FindNode(relationship.From.Model.Id.ToString());
            if (drawingNodeSource is null)
                return;

            var drawingEdge = drawingNodeSource.OutEdges
                .Where(e => e.Target.Equals(relationship.To.Model.Id.ToString()))
                .FirstOrDefault();

            if (drawingEdge is null)
                return;

            // update the underlying label
            drawingEdge.Label.Text = relationship.Name;

            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingEdge, out var edgeViewer))
                this.Invalidate(edgeViewer);

            // remasure the node
            //var tb = new TextBlock { Text = node.Name };
            //tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            //drawingEdge.BoundingBox = new GeometryRectangle(0, 0, tb.DesiredSize.Width, tb.DesiredSize.Height)
            //{
            //    Center = drawingEdge.GeometryNode.BoundingBox.Center
            //};
        }
    }
}