using Microsoft.Msagl.Core;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GeometryRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;

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

                // fill the map
                //   frameworkElementsToDrawingObjects
                // with WPF drawable ibjewt representing the texts shown in on teh canvas.
                this.FillFrameworkElementsFromDrawingObjects();

                if (this.NeedToCalculateLayout)
                {
                    this.Graph.CreateGeometryGraph(); // forcing the layout recalculation
                    this.GraphCanvas.InvokeInUiThread(this.InitializeGeometryGraph);
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

        // The graph items are inspected and for all graph object labels a mapping of
        // DrawingObject -> FrameworkElements is created.
        // The resulting TextBlocks are measured and have attributes set as defined by the
        // MSAGL Label.

        private void FillFrameworkElementsFromDrawingObjects()
        {
            foreach (var drawingEdge in this.Graph.Edges)
                this.FillFrameworkElementsWithEdgeLabels(drawingEdge, out var _);

            foreach (var drawingNode in this.Graph.Nodes)
                this.FillFrameworkElementsWithNodeLabels(drawingNode, out var _);

            if (drawingGraph.RootSubgraph != null)
                foreach (var subgraph in this.Graph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf())
                    this.FillFrameworkElementsWithNodeLabels(subgraph, out var _);
        }

        private void FillFrameworkElementsWithEdgeLabels(Edge drawingEdge, out FrameworkElement fe)
        {
            fe = null;

            var textBlock = this.GraphCanvas
                .InvokeInUiThread(() => CreateTextBlockFromDrawingObjectLabel(drawingEdge.Label));

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingEdge] = textBlock;

            var localEdge = drawingEdge;
            this.GraphCanvas.InvokeInUiThread(() => textBlock.Tag = new VLabel(localEdge, textBlock));

            fe = textBlock;
        }

        private void FillFrameworkElementsWithNodeLabels(Node drawingNode, out FrameworkElement fe)
        {
            fe = null;

            var textBlock = this.GraphCanvas
                .InvokeInUiThread(() => CreateTextBlockFromDrawingObjectLabel(drawingNode.Label));

            if (textBlock is null)
                return;

            this.drawingObjectsToFrameworkElements[drawingNode] = textBlock;
            fe = textBlock;
        }

        public TextBlock CreateTextBlockFromDrawingObjectLabel(Microsoft.Msagl.Drawing.Label drawingLabel)
        {
            return this.SetTextBlockPropertiesFromDrawingLabel(new System.Windows.Controls.TextBlock(), drawingLabel);
        }

        public TextBlock SetTextBlockPropertiesFromDrawingLabel(TextBlock textBlock, Microsoft.Msagl.Drawing.Label drawingLabel)
        {
            Debug.Assert(textBlock.Dispatcher.CheckAccess());

            textBlock.Tag = drawingLabel;
            textBlock.Text = drawingLabel.Text;
            textBlock.FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName);
            textBlock.FontSize = drawingLabel.FontSize;
            textBlock.Foreground = drawingLabel.FontColor.ToWpf();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

            Console.WriteLine($"measure:tb(Height={textBlock.DesiredSize.Height},Width={textBlock.DesiredSize.Width})");

            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;

            //Console.WriteLine($"update:tb(Height={textBlock.Height},Width={textBlock.Width})");

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

            // update the underlying label
            drawingNode.Label.Text = newLabelText;

            var tb = new TextBlock { Text = newLabelText };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var currentBox = drawingNode.GeometryNode.BoundingBox;
            var currentCenter = currentBox.Center;
            var newBox = new GeometryRectangle(0,0, tb.DesiredSize.Width, tb.DesiredSize.Height);
            newBox.Center = currentCenter;
            //newBox.PadWidth((tb.DesiredSize.Width-currentBox.Width)/2);
            drawingNode.GeometryNode.BoundingBox = newBox;

            //this.drawingObjectsToFrameworkElements.TryGetValue(drawingNode, out var frameworkElement);
            //if ((frameworkElement is null) || ((frameworkElement as TextBlock) is null))
            //    return;

            //((TextBlock)frameworkElement).InvokeInUiThread(tb =>
            //{
            //    this.SetTextBlockPropertiesFromDrawingLabel(tb, drawingNode.Label).Arrange(new Rect(tb.DesiredSize));
            //});

            //this.drawingObjectsToIViewerObjects.TryGetValue(drawingNode, out var viewerNode);
            //if ((viewerNode is null) || ((viewerNode as KosmographViewerNode) is null))
            //    return;

            //((KosmographViewerNode)viewerNode).Invalidate();
        }

        #endregion Update a node
    }
}