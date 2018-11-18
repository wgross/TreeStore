using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Kosmograph.Desktop.MsaglGraph
{
    public partial class KosmographViewer
    {
        /// <summary>
        /// The background of the canvas is kept because it is transformed during changes of the canvas size
        /// </summary>
        private System.Windows.Shapes.Rectangle canvasBackgroundRect;

        //private FrameworkElement _rectToFillGraphBackground;

        public event EventHandler LayoutStarted;

        public event EventHandler LayoutComplete;

        public bool RunLayoutAsync { get; set; }

        public bool NeedToCalculateLayout
        {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        private GeometryGraph geometryGraphUnderLayout;

        #region Layout the graph for the first time

        private void LayoutGraph(GeometryGraph geometryGraph)
        {
            if (this.NeedToCalculateLayout)
            {
                try
                {
                    this.LayoutGraph();
                }
                catch (OperationCanceledException)
                {
                    //swallow this exception
                }
            }
        }

        private void PostLayoutStep()
        {
            this.PushDataFromLayoutGraphToFrameworkElements();
            this.backgroundWorker = null; //this will signal that we are not under layout anymore
            this.SetInitialTransform();
        }

        private void PushDataFromLayoutGraphToFrameworkElements()
        {
            this.DrawGraphBackground();
            this.GraphCanvasAddChildren(this.GetViewerNodes().SelectMany(n => { n.Invalidate(); return n.FrameworkElements; }));
            this.GraphCanvasAddChildren(this.GetViewerEdges().SelectMany(e =>
            {
                Panel.SetZIndex(e.EdgeLabelViewer.EdgeLabelVisual, this.ZIndexOfEdge(e.Edge));
                e.Invalidate();
                return e.FrameworkElements;
            }));
        }

        private void InitializeGraphLayout()
        {
            try
            {
                this.GraphCanvasHide();
                this.LayoutGraph();
                this.PostLayoutStep();
                this.GraphChanged?.Invoke(this, null);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.GraphCanvasShow();
            }
            this.LayoutComplete?.Invoke(null, null);
        }

        #endregion Layout the graph for the first time

        #region Update the layout of an already displayed Graph

        public void UpdateGraphLayout()
        {
            try
            {
                this.CreateGeometryGraph();
                this.LayoutGraph();
                this.GraphCanvas.InvokeInUiThread(this.UpdateViewerObjects);
            }
            catch (OperationCanceledException)
            {
            }
            this.LayoutComplete?.Invoke(null, null);
        }

        private void CreateGeometryGraph()
        {
            this.Graph.CreateGeometryGraph();
            foreach (var viewerNode in this.GetViewerNodes())
                viewerNode.UpdateNodeViewerVisuals();
        }

        public void UpdateGraphLayoutInBackground()
        {
            this.LayoutGraphAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    this.GraphCanvas.InvokeInUiThread(this.UpdateViewerObjects);
                }
            });
        }

        private void UpdateViewerObjects()
        {
            // just run everything in the the UI dispatcher.
            // The invlaidates only have to check for an UI affinity instead
            // of waiting to be scheduled.

            foreach (var n in this.GetViewerNodes())
            {
                n.Invalidate();
            }

            foreach (var e in this.GetViewerEdges())
            {
                Panel.SetZIndex(e.EdgeLabelViewer.EdgeLabelVisual, this.ZIndexOfEdge(e.Edge));
                // The drag mark of an edge viewer label is active as long the label
                // is moved away from its original place. A re-layout has to clean this up
                // to remove the dashed line form the label.
                e.EdgeLabelViewer.MarkedForDragging = false;
                e.Invalidate();
            }

            this.GraphChanged?.Invoke(this, null);
        }

        #endregion Update the layout of an already displayed Graph

        #region Common Layout helpers

        private CancelToken LayoutCancelToken { get; set; }

        private void LayoutGraph() => LayoutHelpers.CalculateLayout(this.Graph.GeometryGraph, this.Graph.LayoutAlgorithmSettings, this.LayoutCancelToken);

        private Task LayoutGraphAsync() => Task.Factory.StartNew(this.LayoutGraph);

        #endregion Common Layout helpers

        private bool UnderLayout
        {
            get { return backgroundWorker != null; }
        }

        private void InitializeGraphLayoutInBackground()
        {
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += (a, b) => this.LayoutGraph(this.geometryGraphUnderLayout);
            this.backgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.Error.ToString());
                    this.ClearKosmographViewer();
                }
                else if (LayoutCancelToken.Canceled)
                {
                    this.ClearKosmographViewer();
                }
                else
                {
                    this.GraphCanvas.InvokeInUiThread(() =>
                    {
                        this.GraphCanvasShow();
                        this.PostLayoutStep();
                    });
                }
                this.backgroundWorker = null; //this will signal that we are not under layout anymore

                this.LayoutComplete?.Invoke(null, null);
            };
            this.backgroundWorker.RunWorkerAsync();
        }

        #region Draw the background of the Canvas and the Graph

        private void DrawGraphBackground()
        {
            // at the very back (Z order -2) is a transparent background.
            this.canvasBackgroundRect = this.CreateCanvasBackgroundRect();
            this.GraphCanvas.Children.Add(this.canvasBackgroundRect);

            // behind the graph itself is a rect which has the graphs
            // background color (Z order -1)
            var graphBackground = this.CreateAndPositionGraphBackgroundRectangle();
            if (graphBackground != null)
                this.GraphCanvas.Children.Add(graphBackground);
        }

        private Rectangle CreateCanvasBackgroundRect()
        {
            var parent = (Panel)this.GraphCanvas.Parent;

            var rectangle = new System.Windows.Shapes.Rectangle()
            {
                Width = parent.ActualWidth,
                Height = parent.ActualHeight,
                Fill = System.Windows.Media.Brushes.Transparent
            };

            Canvas.SetLeft(rectangle, 0);
            Canvas.SetTop(rectangle, 0);
            Panel.SetZIndex(rectangle, -2);

            return rectangle;
        }

        private Rectangle CreateAndPositionGraphBackgroundRectangle()
        {
            var rect = this.SetGraphBackgroundSize(this.CreateGraphBackgroundRect());
            if (rect is null)
                return null;

            return rect;
        }

        private Rectangle CreateGraphBackgroundRect()
        {
            var lgGraphBrowsingSettings = drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings;
            if (lgGraphBrowsingSettings is null)
            {
                return this.SetGraphBackgroundSize(new Rectangle());
            }
            else return null;
        }

        private Rectangle SetGraphBackgroundSize(Rectangle graphBackground)
        {
            if (this.GeometryGraph is null)
                return graphBackground;

            // Canvas.SetLeft(_rectToFillGraphBackground, geomGraph.Left);
            // Canvas.SetTop(_rectToFillGraphBackground, geomGraph.Bottom);
            graphBackground.Width = this.GeometryGraph.Width;
            graphBackground.Height = this.GeometryGraph.Height;
            graphBackground.Fill = this.Graph.Attr.BackgroundColor.ToWpf();

            Panel.SetZIndex(graphBackground, -1);

            Wpf2MsaglConverters.PositionFrameworkElement(graphBackground, this.GeometryGraph.BoundingBox.Center, 1);

            return graphBackground;
        }

        #endregion Draw the background of the Canvas and the Graph
    }
}