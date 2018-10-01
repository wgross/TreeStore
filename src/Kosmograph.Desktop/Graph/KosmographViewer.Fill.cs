using Microsoft.Msagl.Core;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DrawingGraph = Microsoft.Msagl.Drawing.Graph;

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

                // fill the map
                //   frameworkElementsToDrawingObjects
                // with WPF drawable ibjewt representing the texts shown in on teh canvas.
                this.FillFrameworkElementsFromDrawingObjects(this.Graph);

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

        private void FillFrameworkElementsFromDrawingObjects(DrawingGraph graph)
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
    }
}