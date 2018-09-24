using Microsoft.Msagl.Core;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private readonly Dictionary<DrawingObject, FrameworkElement> drawingObjectsToFrameworkElements = new Dictionary<DrawingObject, FrameworkElement>();

        #region Fill the viewer with labels for edges and nodes

        private void FillKosmographViewer()
        {
            lock (this.syncRoot)
            {
                this.FillKomsmographViewerImpl();
            }
        }

        private void FillKomsmographViewerImpl()
        {
            try
            {
                // i feel like this should be moved to th elayout8ng in bg of fg....
                this.LayoutStarted?.Invoke(null, null);
                this.CancelToken = new CancelToken();

                if (this.Graph is null)
                    return;

                this.GraphCanvasHide();
                this.ClearKosmographViewerImpl();
                this.FillFrameworkElementsFromDrawingObjectLabels();

                if (this.NeedToCalculateLayout)
                {
                    this.Graph.CreateGeometryGraph(); // forcing the layout recalculation
                    this.GraphCanvas.InvokeInUiThread(PopulateGeometryOfGeometryGraph);
                }

                this.geometryGraphUnderLayout = this.GeometryGraph;
                if (this.RunLayoutAsync)
                    this.SetUpBackgrounWorkerAndRunAsync();
                else
                    this.RunLayoutInUIThread();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        // The graph items are inspected and for all graph objects a mapping of
        // DrawingObject -> FrameworkElements is created.

        private void FillFrameworkElementsFromDrawingObjectLabels()
        {
            foreach (var drawingEdge in this.Graph.Edges)
                this.FillFrameworkElementsFromEdgeLabel(drawingEdge);

            foreach (var drawingNode in this.Graph.Nodes)
                this.FillFrameworkElementFromNodeLabel(drawingNode);

            if (drawingGraph.RootSubgraph != null)
                foreach (var subgraph in this.Graph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf())
                    this.FillFrameworkElementFromNodeLabel(subgraph);
        }

        private void FillFrameworkElementsFromEdgeLabel(Edge drawingEdge)
        {
            var textBlock = this.GraphCanvas
                .InvokeInUiThread(() => CreateTextBlockForMsaglDrawingObjectLabel(drawingEdge.Label));

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingEdge] = textBlock;

            var localEdge = drawingEdge;
            this.GraphCanvas.InvokeInUiThread(() => textBlock.Tag = new VLabel(localEdge, textBlock));
        }

        private void FillFrameworkElementFromNodeLabel(Node drawingNode)
        {
            var textBlock = this.GraphCanvas
                .InvokeInUiThread(() => CreateTextBlockForMsaglDrawingObjectLabel(drawingNode.Label));

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingNode] = textBlock;
        }

        public TextBlock CreateTextBlockForMsaglDrawingObjectLabel(Microsoft.Msagl.Drawing.Label drawingLabel)
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Tag = drawingLabel,
                Text = drawingLabel.Text,
                FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName),
                FontSize = drawingLabel.FontSize,
                Foreground = drawingLabel.FontColor.ToWpf()
            };

            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;
            return textBlock;
        }

        #endregion Fill the viewer with labels for edges and nodes

        #region Clear all nodes and edges from the viewer

        private void ClearKosmographViewer()
        {
            lock (this.syncRoot)
                this.ClearKosmographViewerImpl();
        }

        private void ClearKosmographViewerImpl()
        {
            this.GraphCanvasClearChildren();
            this.drawingObjectsToIViewerObjects.Clear();
            this.drawingObjectsToFrameworkElements.Clear();
        }

        #endregion Clear all nodes and edges from the viewer

        #region Update a node

        public void UpdateNodeLabel(string nodeId, string newLabelText)
        {
            var drawingNode = this.Graph.FindNode(nodeId);
            if (drawingNode is null)
                return;

            drawingNode.LabelText = newLabelText;

            this.drawingObjectsToFrameworkElements.TryGetValue(drawingNode, out var frameworkElement);
            if ((frameworkElement is null) || ((frameworkElement as TextBlock) is null))
                return;

            ((TextBlock)frameworkElement).InvokeInUiThread(() => UpdateTextBlockForMsaglDrawingObjectLabel((TextBlock)frameworkElement, drawingNode.Label));
        }

        public TextBlock UpdateTextBlockForMsaglDrawingObjectLabel(TextBlock textBlock, Microsoft.Msagl.Drawing.Label drawingLabel)
        {
            textBlock.Tag = drawingLabel;
            textBlock.Text = drawingLabel.Text;
            textBlock.FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName);
            textBlock.FontSize = drawingLabel.FontSize;
            textBlock.Foreground = drawingLabel.FontColor.ToWpf();

            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;

            return textBlock;
        }

        #endregion Update a node
    }
}