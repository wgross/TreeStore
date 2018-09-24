using Microsoft.Msagl.Core;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private readonly Object _processGraphLock = new object();

        private void ProcessGraph()
        {
            lock (_processGraphLock)
            {
                this.ProcessGraphUnderLock();
            }
        }

        private void ProcessGraphUnderLock()
        {
            try
            {
                this.LayoutStarted?.Invoke(null, null);
                this.CancelToken = new CancelToken();

                if (this.drawingGraph == null)
                    return;

                this.HideCanvas();
                this.ClearGraphViewer();
                this.CreateFrameworkElementsForLabelsOnly();

                if (NeedToCalculateLayout)
                {
                    drawingGraph.CreateGeometryGraph(); //forcing the layout recalculation
                    if (GraphCanvas.Dispatcher.CheckAccess())
                        PopulateGeometryOfGeometryGraph();
                    else
                        GraphCanvas.Dispatcher.Invoke(PopulateGeometryOfGeometryGraph);
                }

                this.geometryGraphUnderLayout = drawingGraph.GeometryGraph;
                if (RunLayoutAsync)
                    SetUpBackgrounWorkerAndRunAsync();
                else
                    RunLayoutInUIThread();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ClearGraphViewer()
        {
            this.GraphCanvas.InvokeInUiThread(() => GraphCanvas.Children.Clear());
            lock (this)
            {
                this.drawingObjectsToIViewerObjects.Clear();
                this.drawingObjectsToFrameworkElements.Clear();
            }
        }

        private void CreateFrameworkElementsForLabelsOnly()
        {
            foreach (var edge in drawingGraph.Edges)
            {
                var edgesFrameworkElement = this.CreateFrameworkElementForDrawingObject(edge);
                if (edgesFrameworkElement != null)
                {
                    var localEdge = edge;
                    this.GraphCanvas.InvokeInUiThread(() =>
                    {
                        edgesFrameworkElement.Tag = new VLabel(localEdge, edgesFrameworkElement);
                    });
                }
            }

            foreach (var node in drawingGraph.Nodes)
                this.CreateFrameworkElementForDrawingObject(node);

            if (drawingGraph.RootSubgraph != null)
                foreach (var subgraph in drawingGraph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf())
                    this.CreateFrameworkElementForDrawingObject(subgraph);
        }

        private FrameworkElement CreateFrameworkElementForDrawingObject(DrawingObject drawingObject)
        {
            var textBlock = this.CreateTextBlockForMsaglDrawingObject(drawingObject);
            if (textBlock != null)
            {
                lock (this)
                    this.drawingObjectsToFrameworkElements[drawingObject] = textBlock;
            }
            return textBlock;
        }

        private TextBlock CreateTextBlockForMsaglDrawingObject(DrawingObject drawingObj)
        {
            if (drawingObj is Subgraph)
                return null; //todo: add Label support later

            var labeledObj = drawingObj as ILabeledObject;
            if (labeledObj == null)
                return null;

            var drawingLabel = labeledObj.Label;
            if (drawingLabel == null)
                return null;

            return this.GraphCanvas.InvokeInUiThread(() => drawingLabel.ToWpf());
        }
    }
}