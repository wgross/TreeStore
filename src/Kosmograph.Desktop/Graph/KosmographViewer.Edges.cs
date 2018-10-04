using Kosmograph.Desktop.ViewModel;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private IEnumerable<KosmographViewerEdge> CreateViewerEdges()
        {
            foreach (var edge in this.drawingGraph.Edges)
                yield return this.CreateViewerEdge(edge, lgSettings: null);
        }

        private IEnumerable<KosmographViewerEdge> GetOrCreateEdgeViewers()
        {
            foreach (var edge in this.drawingGraph.Edges)
                yield return this.GetViewerEdge(edge);
        }

        private KosmographViewerEdge GetOrCreateEdgeViewer(DrawingEdge drawingEdge, LgLayoutSettings lgSettings)
        {
            lock (this.syncRoot)
            {
                return this.GetViewerEdge(drawingEdge) ?? this.CreateViewerEdge(drawingEdge, lgSettings);
            }
        }

        private KosmographViewerEdge GetViewerEdge(DrawingEdge drawingEdge)
        {
            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingEdge, out var existingEdgeViewer))
                return (KosmographViewerEdge)existingEdgeViewer;
            else return null;
        }

        private KosmographViewerEdge CreateViewerEdge(DrawingEdge drawingEdge, LgLayoutSettings lgSettings)
        {
            if (lgSettings != null)
                return CreateEdgeForLgCase(lgSettings, drawingEdge);

            // fetches the textBlock 'Fill' method
            var labelTextBlock = VisualsFactory.CreateLabel(drawingEdge.Label);
            var viewerEdgeLabel = new KosmographViewerEdgeLabel(drawingEdge.Label, labelTextBlock);
            labelTextBlock.Tag = viewerEdgeLabel;

            var edgeViewer = new KosmographViewerEdge(drawingEdge, viewerEdgeLabel);

            this.drawingObjectsToIViewerObjects[drawingEdge] = edgeViewer;

            return edgeViewer;
        }

        private int ZIndexOfEdge(DrawingEdge edge)
        {
            var source = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.SourceNode];
            var target = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.TargetNode];

            return Math.Max(source.ZIndex, target.ZIndex) + 1;
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