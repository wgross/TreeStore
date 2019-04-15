using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Desktop.ViewModel;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.MsaglGraph
{
    public partial class KosmographViewer
    {
        private void CreateViewerEdges()
        {
            foreach (var edge in this.drawingGraph.Edges)
                this.CreateViewerEdge(edge, lgSettings: null);
        }

        private KosmographViewerEdge CreateViewerEdge(DrawingEdge drawingEdge, LgLayoutSettings lgSettings)
        {
            KosmographViewerEdge edgeViewer = null;

            if (lgSettings is null)
            {
                edgeViewer = new KosmographViewerEdge(drawingEdge, new KosmographViewerEdgeLabel(drawingEdge.Label, VisualsFactory.CreateNodeViewerVisual(drawingEdge.Label)))
                {
                    PathStrokeThicknessFunc = () => GetBorderPathThickness() * drawingEdge.Attr.LineWidth
                };
            }
            if (lgSettings != null)
            {
                edgeViewer = new KosmographViewerEdge(drawingEdge, lgSettings)
                {
                    PathStrokeThicknessFunc = () => GetBorderPathThickness() * drawingEdge.Attr.LineWidth
                };
            }

            return (KosmographViewerEdge)(this.drawingObjectsToIViewerObjects[drawingEdge] = edgeViewer);
        }

        private IEnumerable<KosmographViewerEdge> GetViewerEdges()
        {
            foreach (var edge in this.drawingGraph.Edges)
                yield return this.GetViewerEdge(edge);
        }

        private KosmographViewerEdge GetViewerEdge(DrawingEdge drawingEdge)
        {
            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingEdge, out var existingEdgeViewer))
                return (KosmographViewerEdge)existingEdgeViewer;
            else return null;
        }

        private int ZIndexOfEdge(DrawingEdge edge)
        {
            var source = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.SourceNode];
            var target = (KosmographViewerNode)drawingObjectsToIViewerObjects[edge.TargetNode];

            return Math.Max(source.ZIndex, target.ZIndex) + 1;
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
        }
    }
}