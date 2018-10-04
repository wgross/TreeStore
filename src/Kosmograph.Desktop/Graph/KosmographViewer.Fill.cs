using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrawingGraph = Microsoft.Msagl.Drawing.Graph;
using DrawingLabel = Microsoft.Msagl.Drawing.Label;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private readonly Dictionary<DrawingObject, TextBlock> drawingObjectsToFrameworkElements = new Dictionary<DrawingObject, TextBlock>();

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

                this.Graph.CreateGeometryGraph();

                // fill the map
                //   frameworkElementsToDrawingObjects
                // with WPF drawable ibjewt representing the texts shown in on teh canvas.
                this.PrepareVisualsFromDrawingObjects(this.Graph);
                var nodes = this.CreateViewerNodes().ToArray();
                var edges = this.CreateViewerEdges().ToArray();

                this.InitializeGeometryGraphFromVisuals(this.Graph);

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

        private void PrepareVisualsFromDrawingObjects(DrawingGraph graph)
        {
            foreach (var drawingEdge in graph.Edges)
                this.PrepareEdgeLabels(drawingEdge, out var _);

            foreach (var drawingNode in graph.Nodes)
                this.PrepareNodeLabels(drawingNode, out var _);

            if (drawingGraph.RootSubgraph != null)
                foreach (var subgraph in graph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf())
                    this.PrepareNodeLabels(subgraph, out var _);
        }

        #endregion Fill the viewer with labels for edges and nodes

        #region Push initial state of framework element measurement to the geomotry nodes

        // ONLY SUBGRAPH INITIALIZETION IS LEFT

        /// <summary>
        /// Initialize geometry nodes/edges(subgraphs with measutred sizes of the
        /// prepared Framework Elements
        /// </summary>
        private void InitializeGeometryGraphFromVisuals(DrawingGraph drawingGraph)
        {
            //this.InitializeGeometryGraphNodes(drawingGraph);
            this.InitializeGeometryGraphSubGraphs(drawingGraph);
            //this.InitializeGeometryGraphEdges(drawingGraph);
        }

        //private void InitializeGeometryGraphNodes(DrawingGraph drawingGraph)
        //{
        //    foreach (var drawingNode in drawingGraph.Nodes.Where(n => n.GeometryNode != null))
        //    {
        //        drawingNode.GeometryNode.BoundaryCurve = this.GetNodeBoundaryCurve(drawingNode, this.drawingObjectsToFrameworkElements[drawingNode]);
        //    }
        //}

        private void InitializeGeometryGraphSubGraphs(DrawingGraph geometryGraph)
        {
            foreach (Cluster cluster in geometryGraph.GeometryGraph.RootCluster.AllClustersWideFirstExcludingSelf())
            {
                var subgraph = (Subgraph)cluster.UserData;

                cluster.BoundaryCurve = this.GetClusterCollapsedBoundary(subgraph);

                if (cluster.RectangularBoundary == null)
                    cluster.RectangularBoundary = new RectangularClusterBoundary();

                cluster.RectangularBoundary.TopMargin = subgraph.DiameterOfOpenCollapseButton + 0.5 + subgraph.Attr.LineWidth / 2;
                //AssignLabelWidthHeight(msaglNode, msaglNode.UserData as DrawingObject);
            }
        }

        //private void InitializeGeometryGraphEdges(DrawingGraph geometryGraph)
        //{
        //    foreach (var drawingEdge in geometryGraph.Edges.Where(e => e.GeometryEdge != null))
        //    {
        //        AssignLabelWidthHeight(drawingEdge.GeometryEdge, drawingEdge);
        //    }
        //}

        //private ICurve GetNodeBoundaryCurve(DrawingNode node, FrameworkElement frameworkElement)
        //{
        //    if (frameworkElement is null)
        //        throw new InvalidOperationException($"node({node}) not prepared");

        //    double width, height;

        //    // a Frameworkelement was prerpared beforehand.
        //    width = frameworkElement.Width + 2 * node.Attr.LabelMargin;
        //    height = frameworkElement.Height + 2 * node.Attr.LabelMargin;

        //    // the calculated width must not be smaller the minimal size.

        //    if (width < drawingGraph.Attr.MinNodeWidth)
        //        width = drawingGraph.Attr.MinNodeWidth;
        //    if (height < drawingGraph.Attr.MinNodeHeight)
        //        height = drawingGraph.Attr.MinNodeHeight;

        //    return NodeBoundaryCurves.GetNodeBoundaryCurve(node, width, height);
        //}

        //private void AssignLabelWidthHeight(Microsoft.Msagl.Core.Layout.ILabeledObject labeledGeomObj, DrawingObject drawingObj)
        //{
        //    if (drawingObjectsToFrameworkElements.TryGetValue(drawingObj, out var fe))
        //    {
        //        labeledGeomObj.Label.Width = fe.Width;
        //        labeledGeomObj.Label.Height = fe.Height;
        //    }
        //}

        private ICurve GetClusterCollapsedBoundary(Subgraph subgraph)
        {
            double width, height;

            TextBlock fe;
            if (drawingObjectsToFrameworkElements.TryGetValue(subgraph, out fe))
            {
                width = fe.Width + 2 * subgraph.Attr.LabelMargin + subgraph.DiameterOfOpenCollapseButton;
                height = Math.Max(fe.Height + 2 * subgraph.Attr.LabelMargin, subgraph.DiameterOfOpenCollapseButton);
            }
            else return GetApproximateCollapsedBoundary(subgraph);

            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;
            return NodeBoundaryCurves.GetNodeBoundaryCurve(subgraph, width, height);
        }

        private ICurve GetApproximateCollapsedBoundary(Subgraph subgraph)
        {
            if (textBoxForApproxNodeBoundaries == null)
                SetUpTextBoxForApproxNodeBoundaries();

            double width, height;
            if (String.IsNullOrEmpty(subgraph.LabelText))
                height = width = subgraph.DiameterOfOpenCollapseButton;
            else
            {
                double a = ((double)subgraph.LabelText.Length) / textBoxForApproxNodeBoundaries.Text.Length *
                           subgraph.Label.FontSize / DrawingLabel.DefaultFontSize;
                width = textBoxForApproxNodeBoundaries.Width * a + subgraph.DiameterOfOpenCollapseButton;
                height =
                    Math.Max(
                        textBoxForApproxNodeBoundaries.Height * subgraph.Label.FontSize / DrawingLabel.DefaultFontSize,
                        subgraph.DiameterOfOpenCollapseButton);
            }

            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve(subgraph, width, height);
        }

        #endregion Push initial state of framework element measurement to the geomotry nodes
    }
}