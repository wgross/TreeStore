using Kosmograph.Desktop.ViewModel;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using GeometryRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;

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
            return VisualsFactory.CreateLabel(drawingLabel);
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