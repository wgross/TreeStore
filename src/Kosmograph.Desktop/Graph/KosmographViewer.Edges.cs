using Microsoft.Msagl.Layout.LargeGraphLayout;
using System;
using System.Windows.Controls;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private void FillFrameworkElementsWithEdgeLabels(DrawingEdge drawingEdge, out TextBlock textBlock)
        {
            textBlock = this.GraphCanvas
                .InvokeInUiThread(() => CreateTextBlockFromDrawingObjectLabel(drawingEdge.Label));

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingEdge] = textBlock;

            var edgeLabel_closure = drawingEdge.Label;
            var textBlock_closure = textBlock;

            // sync into UI thread an make the label viewer object

            this.GraphCanvas.InvokeInUiThread(() => textBlock_closure.Tag = new KosmographViewerEdgeLabel(edgeLabel_closure, textBlock_closure));
        }

        private void CreateEdges()
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

                // fetches the textbl.oicj from the 'Fill' method
                TextBlock labelTextBox;
                this.drawingObjectsToFrameworkElements.TryGetValue(edge, out labelTextBox);
                var edgeViewer = new KosmographViewerEdge(edge, (KosmographViewerEdgeLabel)(labelTextBox.Tag));

                var zIndex = this.ZIndexOfEdge(edge);
                drawingObjectsToIViewerObjects[edge] = edgeViewer;

                if (edge.Label != null)
                    this.SetVEdgeLabel(edge, edgeViewer, zIndex);

                Panel.SetZIndex(edgeViewer.EdgePath, zIndex);

                GraphCanvas.Children.Add(edgeViewer.EdgePath);
                this.SetVEdgeArrowheads(edgeViewer, zIndex);

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

        private void SetVEdgeLabel(DrawingEdge edge, KosmographViewerEdge edgeViewer, int zIndex)
        {
            TextBlock frameworkElementForEdgeLabel;
            if (!drawingObjectsToFrameworkElements.TryGetValue(edge, out frameworkElementForEdgeLabel))
            {
                this.FillFrameworkElementsWithEdgeLabels(edge, out frameworkElementForEdgeLabel);
                frameworkElementForEdgeLabel.Tag = new KosmographViewerEdgeLabel(edge.Label, frameworkElementForEdgeLabel);
            }

            //edgeViewer.EdgeLabelViewer = (KosmographViewerEdgeLabel)frameworkElementForEdgeLabel.Tag;
            if (frameworkElementForEdgeLabel.Parent == null)
            {
                GraphCanvas.Children.Add(frameworkElementForEdgeLabel);
                Panel.SetZIndex(frameworkElementForEdgeLabel, zIndex);
            }
        }

        private KosmographViewerEdge CreateEdgeForLgCase(LgLayoutSettings lgSettings, DrawingEdge edge)
        {
            return (KosmographViewerEdge)(drawingObjectsToIViewerObjects[edge] = new KosmographViewerEdge(edge, lgSettings)
            {
                PathStrokeThicknessFunc = () => GetBorderPathThickness() * edge.Attr.LineWidth
            });
        }

        private void SetVEdgeArrowheads(KosmographViewerEdge vEdge, int zIndex)
        {
            if (vEdge.SourceArrowHeadPath != null)
            {
                Panel.SetZIndex(vEdge.SourceArrowHeadPath, zIndex);
                GraphCanvas.Children.Add(vEdge.SourceArrowHeadPath);
            }
            if (vEdge.TargetArrowHeadPath != null)
            {
                Panel.SetZIndex(vEdge.TargetArrowHeadPath, zIndex);
                GraphCanvas.Children.Add(vEdge.TargetArrowHeadPath);
            }
        }
    }
}