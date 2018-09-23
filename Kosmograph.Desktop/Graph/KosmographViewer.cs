/*
Microsoft Automatic Graph Layout,MSAGL

Copyright (c) Microsoft Corporation

All rights reserved.

MIT License

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
""Software""), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using Ellipse = System.Windows.Shapes.Ellipse;
using ILabeledObject = Microsoft.Msagl.Drawing.ILabeledObject;
using Label = Microsoft.Msagl.Drawing.Label;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using ModifierKeys = Microsoft.Msagl.Drawing.ModifierKeys;
using Node = Microsoft.Msagl.Core.Layout.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using Rectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using Size = System.Windows.Size;
using WpfPoint = System.Windows.Point;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer : IViewer
    {
        private Path _targetArrowheadPathForRubberEdge;

        private Path _rubberEdgePath;
        private Path _rubberLinePath;
        private Point _sourcePortLocationForEdgeRouting;

        //WpfPoint _objectUnderMouseDetectionLocation;
        private CancelToken _cancelToken = new CancelToken();

        private BackgroundWorker _backgroundWorker;
        private Point mouseDownPositionInGraph;
        private bool mouseDownPositionInGraph_initialized;

        private Ellipse _sourcePortCircle;
        protected Ellipse TargetPortCircle { get; set; }

        private WpfPoint _objectUnderMouseDetectionLocation;

        public event EventHandler LayoutStarted;

        public event EventHandler LayoutComplete;

        /// <summary>
        /// if set to true will layout in a task
        /// </summary>
        public bool RunLayoutAsync;

        private Microsoft.Msagl.Drawing.Graph drawingGraph;

        private readonly Dictionary<DrawingObject, FrameworkElement> drawingObjectsToFrameworkElements =
            new Dictionary<DrawingObject, FrameworkElement>();

        private readonly LayoutEditor layoutEditor;

        private GeometryGraph geometryGraphUnderLayout;
        /*
                Thread layoutThread;
        */
        private bool needToCalculateLayout = true;

        private static double _dpiX;
        private static int _dpiY;

        private readonly Dictionary<DrawingObject, IViewerObject> drawingObjectsToIViewerObjects =
            new Dictionary<DrawingObject, IViewerObject>();

        private FrameworkElement _rectToFillGraphBackground;
        private System.Windows.Shapes.Rectangle _rectToFillCanvas;

        private GeometryGraph GeometryGraph
        {
            get { return drawingGraph.GeometryGraph; }
        }

        /// <summary>
        /// the canvas to draw the graph
        /// </summary>
        public Canvas GraphCanvas { get; } = new Canvas();

        public KosmographViewer(Canvas graphCanvas, ClickCounter clickCounter)
        {
            this.GraphCanvas = graphCanvas;
            this.clickCounter = clickCounter;

            //LargeGraphNodeCountThreshold = 0;
            layoutEditor = new LayoutEditor(this);

            this.GraphCanvas.SizeChanged += GraphCanvasSizeChanged;
            this.GraphCanvas.MouseLeftButtonDown += GraphCanvasMouseLeftButtonDown;
            this.GraphCanvas.MouseRightButtonDown += GraphCanvasRightMouseDown;
            this.GraphCanvas.MouseMove += GraphCanvasMouseMove;

            this.GraphCanvas.MouseLeftButtonUp += GraphCanvasMouseLeftButtonUp;
            this.GraphCanvas.MouseWheel += GraphCanvasMouseWheel;
            this.GraphCanvas.MouseRightButtonUp += GraphCanvasRightMouseUp;
            this.ViewChangeEvent += AdjustBtrectRenderTransform;

            this.LayoutEditingEnabled = true;
            this.clickCounter.Elapsed += ClickCounterElapsed;
        }

        #region WPF stuff

        /// <summary>
        /// adds the main panel of the viewer to the children of the parent
        /// </summary>
        /// <param name="panel"></param>
        public void BindToPanel(Panel panel)
        {
            panel.Children.Add(GraphCanvas);
            GraphCanvas.UpdateLayout();
        }

        private void ClickCounterElapsed(object sender, EventArgs e)
        {
            var vedge = this.clickCounter.ClickedObject as VEdge;
            if (vedge != null)
            {
                if (this.clickCounter.UpCount == this.clickCounter.DownCount && this.clickCounter.UpCount == 1)
                    this.HandleClickForEdge(vedge);
            }
            this.clickCounter.ClickedObject = null;
        }

        private void AdjustBtrectRenderTransform(object sender, EventArgs e)
        {
            if (_rectToFillCanvas == null)
                return;
            _rectToFillCanvas.RenderTransform = (Transform)GraphCanvas.RenderTransform.Inverse;
            var parent = (Panel)GraphCanvas.Parent;
            _rectToFillCanvas.Width = parent.ActualWidth;
            _rectToFillCanvas.Height = parent.ActualHeight;
        }

        private void HandleClickForEdge(VEdge vEdge)
        {
            //todo : add a hook
            var lgSettings = Graph.LayoutAlgorithmSettings as LgLayoutSettings;
            if (lgSettings != null)
            {
                var lgEi = lgSettings.GeometryEdgesToLgEdgeInfos[vEdge.Edge.GeometryEdge];
                lgEi.SlidingZoomLevel = lgEi.SlidingZoomLevel != 0 ? 0 : double.PositiveInfinity;

                ViewChangeEvent(null, null);
            }
        }

        public LayoutEditor LayoutEditor
        {
            get { return layoutEditor; }
        }

        // Return the result of the hit test to the callback.

        private FrameworkElement GetFrameworkElementFromIViewerObject(IViewerObject viewerObject)
        {
            FrameworkElement ret;

            var vNode = viewerObject as VNode;
            if (vNode != null) ret = vNode.FrameworkElementOfNodeForLabel ?? vNode.BoundaryPath;
            else
            {
                var vLabel = viewerObject as VLabel;
                if (vLabel != null) ret = vLabel.FrameworkElement;
                else
                {
                    var vEdge = viewerObject as VEdge;
                    if (vEdge != null) ret = vEdge.CurvePath;
                    else
                    {
                        throw new InvalidOperationException(
#if DEBUG
                            "Unexpected object type in GraphViewer"
#endif
                            );
                    }
                }
            }
            if (ret == null)
                throw new InvalidOperationException(
#if DEBUG
                    "did not find a framework element!"
#endif
                    );

            return ret;
        }

        // Return the result of the hit test to the callback.
        private HitTestResultBehavior MyHitTestResultCallbackWithNoCallbacksToTheUser(HitTestResult result)
        {
            var frameworkElement = result.VisualHit as FrameworkElement;

            if (frameworkElement == null)
                return HitTestResultBehavior.Continue;
            object tag = frameworkElement.Tag;
            if (tag != null)
            {
                //it is a tagged element
                var ivo = tag as IViewerObject;
                if (ivo != null)
                {
                    if (ivo.DrawingObject.IsVisible)
                    {
                        objectUnderMouseCursor = ivo;
                        if (tag is VNode || tag is Label)
                            return HitTestResultBehavior.Stop;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(tag is Rail);
                    objectUnderMouseCursor = tag;
                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
        }

        /// <summary>
        /// this function pins the sourcePoint to screenPoint
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="sourcePoint"></param>
        private void SetTransformFromTwoPoints(WpfPoint screenPoint, Point sourcePoint)
        {
            var scale = CurrentScale;
            SetTransform(scale, screenPoint.X - scale * sourcePoint.X, screenPoint.Y + scale * sourcePoint.Y);
        }

        /// <summary>
        /// Moves the point to the center of the viewport
        /// </summary>
        /// <param name="sourcePoint"></param>
        public void PointToCenter(Point sourcePoint)
        {
            WpfPoint center = new WpfPoint(GraphCanvas.RenderSize.Width / 2, GraphCanvas.RenderSize.Height / 2);
            SetTransformFromTwoPoints(center, sourcePoint);
        }

        public void NodeToCenterWithScale(Microsoft.Msagl.Drawing.Node node, double scale)
        {
            if (node.GeometryNode == null) return;
            var screenPoint = new WpfPoint(GraphCanvas.RenderSize.Width / 2, GraphCanvas.RenderSize.Height / 2);
            var sourcePoint = node.BoundingBox.Center;
            SetTransform(scale, screenPoint.X - scale * sourcePoint.X, screenPoint.Y + scale * sourcePoint.Y);
        }

        public void NodeToCenter(Microsoft.Msagl.Drawing.Node node)
        {
            if (node.GeometryNode == null) return;
            PointToCenter(node.GeometryNode.Center);
        }

        //        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        //        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        //        public static extern bool SetCursorPos(int X, int Y);

        public double CurrentScale
        {
            get { return ((MatrixTransform)GraphCanvas.RenderTransform).Matrix.M11; }
        }

        /*
                void Pan(Point vector) {
                    graphCanvas.RenderTransform = new MatrixTransform(mouseDownTransform[0, 0], mouseDownTransform[0, 1],
                                                                      mouseDownTransform[1, 0], mouseDownTransform[1, 1],
                                                                      mouseDownTransform[0, 2] +vector.X,
                                                                      mouseDownTransform[1, 2] +vector.Y);
                }
        */

        protected double CurrentXOffset
        {
            get { return ((MatrixTransform)GraphCanvas.RenderTransform).Matrix.OffsetX; }
        }

        protected double CurrentYOffset
        {
            get { return ((MatrixTransform)GraphCanvas.RenderTransform).Matrix.OffsetY; }
        }

        #endregion WPF stuff

        #region IViewer stuff

        public event EventHandler<EventArgs> ViewChangeEvent;

        public event EventHandler<MsaglMouseEventArgs> MouseDown;

        public event EventHandler<MsaglMouseEventArgs> MouseMove;

        public event EventHandler<MsaglMouseEventArgs> MouseUp;

        private IViewerObject GetIViewerObjectFromObjectUnderCursor(object obj)
        {
            if (obj == null)
                return null;
            return obj as IViewerObject;
        }

        public void Invalidate(IViewerObject objectToInvalidate) => ((IInvalidatable)objectToInvalidate).Invalidate();

        public void Invalidate()
        {
            //todo: is it right to do nothing
        }

        public event EventHandler GraphChanged;

        public ModifierKeys ModifierKeys
        {
            get
            {
                switch (Keyboard.Modifiers)
                {
                    case System.Windows.Input.ModifierKeys.Alt:
                        return ModifierKeys.Alt;

                    case System.Windows.Input.ModifierKeys.Control:
                        return ModifierKeys.Control;

                    case System.Windows.Input.ModifierKeys.None:
                        return ModifierKeys.None;

                    case System.Windows.Input.ModifierKeys.Shift:
                        return ModifierKeys.Shift;

                    case System.Windows.Input.ModifierKeys.Windows:
                        return ModifierKeys.Windows;

                    default:
                        return ModifierKeys.None;
                }
            }
        }

        public Point ScreenToSource(MsaglMouseEventArgs e)
        {
            var p = new Point(e.X, e.Y);
            var m = Transform.Inverse;
            return m * p;
        }

        public IEnumerable<IViewerObject> Entities
        {
            get
            {
                foreach (var viewerObject in drawingObjectsToIViewerObjects.Values)
                {
                    yield return viewerObject;
                    var edge = viewerObject as VEdge;
                    if (edge != null)
                        if (edge.VLabel != null)
                            yield return edge.VLabel;
                }
            }
        }

        internal static double DpiXStatic
        {
            get
            {
                if (_dpiX == 0)
                    GetDpi();
                return _dpiX;
            }
        }

        private static void GetDpi()
        {
            int hdcSrc = NativeMethods.GetWindowDC(NativeMethods.GetDesktopWindow());
            //LOGPIXELSX = 88,
            //LOGPIXELSY = 90,
            _dpiX = NativeMethods.GetDeviceCaps(hdcSrc, 88);
            _dpiY = NativeMethods.GetDeviceCaps(hdcSrc, 90);
            NativeMethods.ReleaseDC(NativeMethods.GetDesktopWindow(), hdcSrc);
        }

        public double DpiX
        {
            get { return DpiXStatic; }
        }

        public double DpiY
        {
            get { return DpiYStatic; }
        }

        private static double DpiYStatic
        {
            get
            {
                if (_dpiX == 0)
                    GetDpi();
                return _dpiY;
            }
        }

        public void OnDragEnd(IEnumerable<IViewerObject> changedObjects)
        {
            throw new NotImplementedException();
        }

        public double LineThicknessForEditing { get; set; }

        /// <summary>
        /// the layout editing with the mouse is enabled if and only if this field is set to false
        /// </summary>
        public bool LayoutEditingEnabled { get; set; }

        public bool InsertingEdge { get; set; }

        public void PopupMenus(params Tuple<string, VoidDelegate>[] menuItems)
        {
            var contextMenu = new ContextMenu();
            foreach (var pair in menuItems)
                contextMenu.Items.Add(CreateMenuItem(pair.Item1, pair.Item2));
            contextMenu.Closed += ContextMenuClosed;
            ContextMenuService.SetContextMenu(GraphCanvas, contextMenu);
        }

        private void ContextMenuClosed(object sender, RoutedEventArgs e)
        {
            ContextMenuService.SetContextMenu(GraphCanvas, null);
        }

        public static object CreateMenuItem(string title, VoidDelegate voidVoidDelegate)
        {
            var menuItem = new MenuItem { Header = title };
            menuItem.Click += (RoutedEventHandler)(delegate { voidVoidDelegate(); });
            return menuItem;
        }

        public double UnderlyingPolylineCircleRadius
        {
            get { return 0.1 * DpiX / CurrentScale; }
        }

        public Microsoft.Msagl.Drawing.Graph Graph
        {
            get { return drawingGraph; }
            set
            {
                drawingGraph = value;
                if (drawingGraph != null)
                    Console.WriteLine("starting processing a graph with {0} nodes and {1} edges", drawingGraph.NodeCount,
                        drawingGraph.EdgeCount);
                ProcessGraph();
            }
        }

        //
        //        void Dumpxy() {
        //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\tmp\dumpxy")) {
        //                file.WriteLine("~nodes");
        //                foreach (var node in Graph.Nodes) {
        //                    var c = node.GeometryNode.Center;
        //                    file.WriteLine("{0} {1} {2}", node.Id, c.X, c.Y);
        //                }
        //                file.WriteLine("~edges");
        //                foreach (var edge in Graph.Edges)
        //                {
        //                    file.WriteLine("{0} {1}", edge.Source, edge.Target);
        //                }
        //            }
        //        }

        private const double DesiredPathThicknessInInches = 0.008;

        private readonly Dictionary<DrawingObject, Func<DrawingObject, FrameworkElement>> registeredCreators =
            new Dictionary<DrawingObject, Func<DrawingObject, FrameworkElement>>();

        private readonly ClickCounter clickCounter;
        public string MsaglFileToSave;

        private double GetBorderPathThickness()
        {
            return DesiredPathThicknessInInches * DpiX;
        }

        private readonly Object _processGraphLock = new object();

        private void ProcessGraph()
        {
            lock (_processGraphLock)
            {
                ProcessGraphUnderLock();
            }
        }

        private void ProcessGraphUnderLock()
        {
            try
            {
                if (LayoutStarted != null)
                    LayoutStarted(null, null);

                CancelToken = new CancelToken();

                if (drawingGraph == null) return;

                HideCanvas();
                ClearGraphViewer();
                CreateFrameworkElementsForLabelsOnly();
                if (NeedToCalculateLayout)
                {
                    drawingGraph.CreateGeometryGraph(); //forcing the layout recalculation
                    if (GraphCanvas.Dispatcher.CheckAccess())
                        PopulateGeometryOfGeometryGraph();
                    else
                        GraphCanvas.Dispatcher.Invoke(PopulateGeometryOfGeometryGraph);
                }

                geometryGraphUnderLayout = drawingGraph.GeometryGraph;
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

        private void RunLayoutInUIThread()
        {
            LayoutGraph();
            PostLayoutStep();
            if (LayoutComplete != null)
                LayoutComplete(null, null);
        }

        private void SetUpBackgrounWorkerAndRunAsync()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += (a, b) => LayoutGraph();
            _backgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show(args.Error.ToString());
                    ClearGraphViewer();
                }
                else if (CancelToken.Canceled)
                {
                    ClearGraphViewer();
                }
                else
                {
                    if (GraphCanvas.Dispatcher.CheckAccess())
                        PostLayoutStep();
                    else
                        GraphCanvas.Dispatcher.Invoke(PostLayoutStep);
                }
                _backgroundWorker = null; //this will signal that we are not under layout anymore

                this.LayoutComplete?.Invoke(null, null);
            };
            _backgroundWorker.RunWorkerAsync();
        }

        private void HideCanvas()
        {
            if (GraphCanvas.Dispatcher.CheckAccess())
                GraphCanvas.Visibility = Visibility.Hidden; // hide canvas while we lay it out asynchronously.
            else
                GraphCanvas.Dispatcher.Invoke(() => GraphCanvas.Visibility = Visibility.Hidden);
        }

        private void LayoutGraph()
        {
            if (NeedToCalculateLayout)
            {
                try
                {
                    LayoutHelpers.CalculateLayout(geometryGraphUnderLayout, drawingGraph.LayoutAlgorithmSettings,
                                                  CancelToken);
                    if (MsaglFileToSave != null)
                    {
                        drawingGraph.Write(MsaglFileToSave);
                        Console.WriteLine("saved into {0}", MsaglFileToSave);
                        Environment.Exit(0);
                    }
                }
                catch (OperationCanceledException)
                {
                    //swallow this exception
                }
            }
        }

        private void PostLayoutStep()
        {
            GraphCanvas.Visibility = Visibility.Visible;
            PushDataFromLayoutGraphToFrameworkElements();
            _backgroundWorker = null; //this will signal that we are not under layout anymore
            if (GraphChanged != null)
                GraphChanged(this, null);

            SetInitialTransform();
        }

        /*
                void SubscribeToChangeVisualsEvents() {
                    //            foreach(var cluster in drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf())
                    //                cluster.Attr.VisualsChanged += AttrVisualsChanged;
                    foreach (var edge in drawingGraph.Edges) {
                        DrawingEdge edge1 = edge;
                        edge.Attr.VisualsChanged += (a, b) => AttrVisualsChangedForEdge(edge1);
                    }

                    foreach (var node in drawingGraph.Nodes) {
                        Drawing.Node node1 = node;
                        node.Attr.VisualsChanged += (a, b) => AttrVisualsChangedForNode(node1);
                    }
                }
        */

        /*
                void AttrVisualsChangedForNode(Drawing.Node node) {
                    IViewerObject viewerObject;
                    if (drawingObjectsToIViewerObjects.TryGetValue(node, out viewerObject)) {
                        var vNode = (VNode) viewerObject;
                        if (vNode != null)
                            vNode.Invalidate();
                    }
                }
        */

        //        void SetupTimerOnViewChangeEvent(object sender, EventArgs e) {
        //            SetupRoutingTimer();
        //        }

        /*
                void TestCorrectness(GeometryGraph oGraph, Set<Drawing.Node> oDrawingNodes, Set<DrawingEdge> oDrawgingEdges) {
                    if (Entities.Count() != oGraph.Nodes.Count + oGraph.Edges.Count) {
                        foreach (var newDrawingNode in oDrawingNodes) {
                            if (!drawingObjectsToIViewerObjects.ContainsKey(newDrawingNode))
                                Console.WriteLine();
                        }
                        foreach (var drawingEdge in oDrawgingEdges) {
                            if (!drawingObjectsToIViewerObjects.ContainsKey(drawingEdge))
                                Console.WriteLine();
                        }
                        foreach (var viewerObject in Entities) {
                            if (viewerObject is VEdge) {
                                Debug.Assert(oDrawgingEdges.Contains(viewerObject.DrawingObject));
                            } else {
                                if (viewerObject is VNode) {
                                    Debug.Assert(oDrawingNodes.Contains(viewerObject.DrawingObject));
                                } else {
                                    Debug.Fail("expecting a node or an edge");
                                }
                            }
                        }
                    }
                }
        */

        /// <summary>
        /// creates a viewer node
        /// </summary>
        /// <param name="drawingNode"></param>
        /// <returns></returns>
        public IViewerNode CreateIViewerNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            var frameworkElement = CreateTextBlockForDrawingObj(drawingNode);
            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;
            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
            drawingNode.GeometryNode = new Node(bc, drawingNode);
            var vNode = CreateVNode(drawingNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);
            return vNode;
        }

        private void ClearGraphViewer()
        {
            ClearGraphCanvasChildren();

            drawingObjectsToIViewerObjects.Clear();
            drawingObjectsToFrameworkElements.Clear();
        }

        private void ClearGraphCanvasChildren()
        {
            if (GraphCanvas.Dispatcher.CheckAccess())
                GraphCanvas.Children.Clear();
            else GraphCanvas.Dispatcher.Invoke(() => GraphCanvas.Children.Clear());
        }

        /*
                void StartLayoutCalculationInThread() {
                    PushGeometryIntoLayoutGraph();
                    graphCanvas.RaiseEvent(new RoutedEventArgs(LayoutStartEvent));

                    layoutThread =
                        new Thread(
                            () =>
                            LayoutHelpers.CalculateLayout(geometryGraphUnderLayout, graph.LayoutAlgorithmSettings));

                    layoutThread.Start();

                    //the timer monitors the thread and then pushes the data from layout graph to the framework
                    layoutThreadCheckingTimer.IsEnabled = true;
                    layoutThreadCheckingTimer.Tick += LayoutThreadCheckingTimerTick;
                    layoutThreadCheckingTimer.Interval = new TimeSpan((long) 10e6);
                    layoutThreadCheckingTimer.Start();
                }
        */

        /*
                void LayoutThreadCheckingTimerTick(object sender, EventArgs e) {
                    if (layoutThread.IsAlive)
                        return;

                    if (Monitor.TryEnter(layoutThreadCheckingTimer)) {
                        if (layoutThreadCheckingTimer.IsEnabled == false)
                            return; //somehow it is called on more time after stopping and disabling
                        layoutThreadCheckingTimer.Stop();
                        layoutThreadCheckingTimer.IsEnabled = false;

                        TransferLayoutDataToWpf();

                        graphCanvas.RaiseEvent(new RoutedEventArgs(LayoutEndEvent));
                        if (LayoutComplete != null)
                            LayoutComplete(this, new EventArgs());
                    }
                }
        */

        //        void TransferLayoutDataToWpf() {
        //            PushDataFromLayoutGraphToFrameworkElements();
        //            graphCanvas.Visibility = Visibility.Visible;
        //            SetInitialTransform();
        //        }
        /// <summary>
        /// zooms to the default view
        /// </summary>
        public void SetInitialTransform()
        {
            if (drawingGraph == null || GeometryGraph == null) return;

            var scale = FitFactor;
            var graphCenter = GeometryGraph.BoundingBox.Center;
            var vp = new Rectangle(new Point(0, 0),
                                   new Point(GraphCanvas.RenderSize.Width, GraphCanvas.RenderSize.Height));

            SetTransformOnViewportWithoutRaisingViewChangeEvent(scale, graphCenter, vp);
        }

        public Image DrawImage(string fileName)
        {
            var ltrans = GraphCanvas.LayoutTransform;
            var rtrans = GraphCanvas.RenderTransform;
            GraphCanvas.LayoutTransform = null;
            GraphCanvas.RenderTransform = null;
            var renderSize = GraphCanvas.RenderSize;

            double scale = FitFactor;
            int w = (int)(GeometryGraph.Width * scale);
            int h = (int)(GeometryGraph.Height * scale);

            SetTransformOnViewportWithoutRaisingViewChangeEvent(scale, GeometryGraph.BoundingBox.Center, new Rectangle(0, 0, w, h));

            Size size = new Size(w, h);
            // Measure and arrange the surface
            // VERY IMPORTANT
            GraphCanvas.Measure(size);
            GraphCanvas.Arrange(new Rect(size));

            foreach (var node in drawingGraph.Nodes.Concat(drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf()))
            {
                IViewerObject o;
                if (drawingObjectsToIViewerObjects.TryGetValue(node, out o))
                {
                    ((VNode)o).Invalidate();
                }
            }

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(w, h, DpiX, DpiY, PixelFormats.Pbgra32);
            renderBitmap.Render(GraphCanvas);

            if (fileName != null)
                // Create a file stream for saving image
                using (System.IO.FileStream outStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                {
                    // Use png encoder for our data
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    // push the rendered bitmap to it
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    // save the data to the stream
                    encoder.Save(outStream);
                }

            GraphCanvas.LayoutTransform = ltrans;
            GraphCanvas.RenderTransform = rtrans;
            GraphCanvas.Measure(renderSize);
            GraphCanvas.Arrange(new Rect(renderSize));

            return new Image { Source = renderBitmap };
        }

        private void SetTransformOnViewportWithoutRaisingViewChangeEvent(double scale, Point graphCenter, Rectangle vp)
        {
            var dx = vp.Width / 2 - scale * graphCenter.X;
            var dy = vp.Height / 2 + scale * graphCenter.Y;

            SetTransformWithoutRaisingViewChangeEvent(scale, dx, dy);
        }

        /*
                void FixArrowheads(LgLayoutSettings lgSettings) {
                    const double arrowheadRatioToBoxDiagonal = 0.3;
                    var maximalArrowheadLength = lgSettings.MaximalArrowheadLength();
                    if (lgSettings.OGraph == null) return;
                    foreach (Edge geomEdge in lgSettings.OGraph.Edges) {
                        var edge = (DrawingEdge) geomEdge.UserData;
                        var vEdge = (VEdge) drawingObjectsToIViewerObjects[edge];

                        if (geomEdge.EdgeGeometry.SourceArrowhead != null) {
                            var origLength = vEdge.EdgeAttrClone.ArrowheadLength;
                            geomEdge.EdgeGeometry.SourceArrowhead.Length =
                                Math.Min(Math.Min(origLength, maximalArrowheadLength),
                                         geomEdge.Source.BoundingBox.Diagonal*arrowheadRatioToBoxDiagonal);
                        }
                        if (geomEdge.EdgeGeometry.TargetArrowhead != null) {
                            var origLength = vEdge.EdgeAttrClone.ArrowheadLength;
                            geomEdge.EdgeGeometry.TargetArrowhead.Length =
                                Math.Min(Math.Min(origLength, maximalArrowheadLength),
                                         geomEdge.Target.BoundingBox.Diagonal*arrowheadRatioToBoxDiagonal);
                        }
                    }
                }
        */

        public Rectangle ClientViewportMappedToGraph
        {
            get
            {
                var t = Transform.Inverse;
                var p0 = new Point(0, 0);
                var p1 = new Point(GraphCanvas.RenderSize.Width, GraphCanvas.RenderSize.Height);
                return new Rectangle(t * p0, t * p1);
            }
        }

        private void SetTransform(double scale, double dx, double dy)
        {
            if (ScaleIsOutOfRange(scale)) return;
            GraphCanvas.RenderTransform = new MatrixTransform(scale, 0, 0, -scale, dx, dy);
            if (ViewChangeEvent != null)
                ViewChangeEvent(null, null);
        }

        private void SetTransformWithoutRaisingViewChangeEvent(double scale, double dx, double dy)
        {
            if (ScaleIsOutOfRange(scale)) return;
            GraphCanvas.RenderTransform = new MatrixTransform(scale, 0, 0, -scale, dx, dy);
        }

        private bool ScaleIsOutOfRange(double scale)
        {
            return scale < 0.000001 || scale > 100000.0; //todo: remove hardcoded values
        }

        private void PushDataFromLayoutGraphToFrameworkElements()
        {
            CreateRectToFillCanvas();
            CreateAndPositionGraphBackgroundRectangle();
            CreateVNodes();
            CreateEdges();
        }

        private void CreateRectToFillCanvas()
        {
            var parent = (Panel)GraphCanvas.Parent;
            _rectToFillCanvas = new System.Windows.Shapes.Rectangle();
            Canvas.SetLeft(_rectToFillCanvas, 0);
            Canvas.SetTop(_rectToFillCanvas, 0);
            _rectToFillCanvas.Width = parent.ActualWidth;
            _rectToFillCanvas.Height = parent.ActualHeight;

            _rectToFillCanvas.Fill = Brushes.Transparent;
            Panel.SetZIndex(_rectToFillCanvas, -2);
            GraphCanvas.Children.Add(_rectToFillCanvas);
        }

        private void CreateEdges()
        {
            foreach (var edge in drawingGraph.Edges)
                CreateEdge(edge, null);
        }

        private VEdge CreateEdge(DrawingEdge edge, LgLayoutSettings lgSettings)
        {
            lock (this)
            {
                if (drawingObjectsToIViewerObjects.ContainsKey(edge))
                    return (VEdge)drawingObjectsToIViewerObjects[edge];
                if (lgSettings != null)
                    return CreateEdgeForLgCase(lgSettings, edge);

                FrameworkElement labelTextBox;
                drawingObjectsToFrameworkElements.TryGetValue(edge, out labelTextBox);
                var vEdge = new VEdge(edge, labelTextBox);

                var zIndex = ZIndexOfEdge(edge);
                drawingObjectsToIViewerObjects[edge] = vEdge;

                if (edge.Label != null)
                    SetVEdgeLabel(edge, vEdge, zIndex);

                Panel.SetZIndex(vEdge.CurvePath, zIndex);
                GraphCanvas.Children.Add(vEdge.CurvePath);
                SetVEdgeArrowheads(vEdge, zIndex);

                return vEdge;
            }
        }

        private int ZIndexOfEdge(DrawingEdge edge)
        {
            var source = (VNode)drawingObjectsToIViewerObjects[edge.SourceNode];
            var target = (VNode)drawingObjectsToIViewerObjects[edge.TargetNode];

            var zIndex = Math.Max(source.ZIndex, target.ZIndex) + 1;
            return zIndex;
        }

        private VEdge CreateEdgeForLgCase(LgLayoutSettings lgSettings, DrawingEdge edge)
        {
            return (VEdge)(drawingObjectsToIViewerObjects[edge] = new VEdge(edge, lgSettings)
            {
                PathStrokeThicknessFunc = () => GetBorderPathThickness() * edge.Attr.LineWidth
            });
        }

        private void SetVEdgeLabel(DrawingEdge edge, VEdge vEdge, int zIndex)
        {
            FrameworkElement frameworkElementForEdgeLabel;
            if (!drawingObjectsToFrameworkElements.TryGetValue(edge, out frameworkElementForEdgeLabel))
            {
                drawingObjectsToFrameworkElements[edge] =
                    frameworkElementForEdgeLabel = CreateTextBlockForDrawingObj(edge);
                frameworkElementForEdgeLabel.Tag = new VLabel(edge, frameworkElementForEdgeLabel);
            }

            vEdge.VLabel = (VLabel)frameworkElementForEdgeLabel.Tag;
            if (frameworkElementForEdgeLabel.Parent == null)
            {
                GraphCanvas.Children.Add(frameworkElementForEdgeLabel);
                Panel.SetZIndex(frameworkElementForEdgeLabel, zIndex);
            }
        }

        private void SetVEdgeArrowheads(VEdge vEdge, int zIndex)
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

        private void CreateVNodes()
        {
            foreach (var node in drawingGraph.Nodes.Concat(drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf()))
            {
                CreateVNode(node);
                Invalidate(drawingObjectsToIViewerObjects[node]);
            }
        }

        private IViewerNode CreateVNode(Microsoft.Msagl.Drawing.Node node)
        {
            lock (this)
            {
                if (drawingObjectsToIViewerObjects.ContainsKey(node))
                    return (IViewerNode)drawingObjectsToIViewerObjects[node];

                FrameworkElement feOfLabel;
                if (!drawingObjectsToFrameworkElements.TryGetValue(node, out feOfLabel))
                    feOfLabel = CreateAndRegisterFrameworkElementOfDrawingNode(node);

                var vn = new VNode(node, feOfLabel,
                    e => (VEdge)drawingObjectsToIViewerObjects[e], () => GetBorderPathThickness() * node.Attr.LineWidth);

                foreach (var fe in vn.FrameworkElements)
                    GraphCanvas.Children.Add(fe);

                drawingObjectsToIViewerObjects[node] = vn;

                #region commented out animation

                /* //playing with the animation
                p.Fill = Brushes.Green;

                SolidColorBrush brush = new SolidColorBrush();
                p.Fill = brush;
                ColorAnimation ca = new ColorAnimation(Colors.Green, Colors.White, new Duration(TimeSpan.FromMilliseconds(3000)));
                //Storyboard sb = new Storyboard();
                //Storyboard.SetTargetProperty(ca, new PropertyPath("Color"));
                //Storyboard.SetTarget(ca, brush);
                //sb.Children.Add(ca);
                //sb.Begin(p);
                brush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                */

                #endregion commented out animation

                return vn;
            }
        }

        public FrameworkElement CreateAndRegisterFrameworkElementOfDrawingNode(Microsoft.Msagl.Drawing.Node node)
        {
            lock (this)
                return drawingObjectsToFrameworkElements[node] = CreateTextBlockForDrawingObj(node);
        }

        private void CreateAndPositionGraphBackgroundRectangle()
        {
            CreateGraphBackgroundRect();
            SetBackgroundRectanglePositionAndSize();

            var rect = _rectToFillGraphBackground as System.Windows.Shapes.Rectangle;
            if (rect != null)
            {
                rect.Fill = this.drawingGraph.Attr.BackgroundColor.ToWpf();
                //rect.Fill = Brushes.Green;
            }
            Panel.SetZIndex(_rectToFillGraphBackground, -1);
            GraphCanvas.Children.Add(_rectToFillGraphBackground);
        }

        private void CreateGraphBackgroundRect()
        {
            var lgGraphBrowsingSettings = drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings;
            if (lgGraphBrowsingSettings == null)
            {
                _rectToFillGraphBackground = new System.Windows.Shapes.Rectangle();
            }
        }

        private void SetBackgroundRectanglePositionAndSize()
        {
            if (GeometryGraph == null) return;
            //            Canvas.SetLeft(_rectToFillGraphBackground, geomGraph.Left);
            //            Canvas.SetTop(_rectToFillGraphBackground, geomGraph.Bottom);
            _rectToFillGraphBackground.Width = GeometryGraph.Width;
            _rectToFillGraphBackground.Height = GeometryGraph.Height;

            var center = GeometryGraph.BoundingBox.Center;
            Wpf2MsaglConverters.PositionFrameworkElement(_rectToFillGraphBackground, center, 1);
        }

        private void PopulateGeometryOfGeometryGraph()
        {
            geometryGraphUnderLayout = drawingGraph.GeometryGraph;
            foreach (
                Node msaglNode in
                    geometryGraphUnderLayout.Nodes)
            {
                var node = (Microsoft.Msagl.Drawing.Node)msaglNode.UserData;
                if (GraphCanvas.Dispatcher.CheckAccess())
                    msaglNode.BoundaryCurve = GetNodeBoundaryCurve(node);
                else
                {
                    var msagNodeInThread = msaglNode;
                    GraphCanvas.Dispatcher.Invoke(() => msagNodeInThread.BoundaryCurve = GetNodeBoundaryCurve(node));
                }
                //AssignLabelWidthHeight(msaglNode, msaglNode.UserData as DrawingObject);
            }

            foreach (
                Cluster cluster in geometryGraphUnderLayout.RootCluster.AllClustersWideFirstExcludingSelf())
            {
                var subgraph = (Subgraph)cluster.UserData;
                if (GraphCanvas.Dispatcher.CheckAccess())
                    cluster.CollapsedBoundary = GetClusterCollapsedBoundary(subgraph);
                else
                {
                    var clusterInThread = cluster;
                    GraphCanvas.Dispatcher.Invoke(
                        () => clusterInThread.BoundaryCurve = GetClusterCollapsedBoundary(subgraph));
                }
                if (cluster.RectangularBoundary == null)
                    cluster.RectangularBoundary = new RectangularClusterBoundary();
                cluster.RectangularBoundary.TopMargin = subgraph.DiameterOfOpenCollapseButton + 0.5 +
                                                        subgraph.Attr.LineWidth / 2;
                //AssignLabelWidthHeight(msaglNode, msaglNode.UserData as DrawingObject);
            }

            foreach (var msaglEdge in geometryGraphUnderLayout.Edges)
            {
                var drawingEdge = (DrawingEdge)msaglEdge.UserData;
                AssignLabelWidthHeight(msaglEdge, drawingEdge);
            }
        }

        private ICurve GetClusterCollapsedBoundary(Subgraph subgraph)
        {
            double width, height;

            FrameworkElement fe;
            if (drawingObjectsToFrameworkElements.TryGetValue(subgraph, out fe))
            {
                width = fe.Width + 2 * subgraph.Attr.LabelMargin + subgraph.DiameterOfOpenCollapseButton;
                height = Math.Max(fe.Height + 2 * subgraph.Attr.LabelMargin, subgraph.DiameterOfOpenCollapseButton);
            }
            else
                return GetApproximateCollapsedBoundary(subgraph);

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
                           subgraph.Label.FontSize / Label.DefaultFontSize;
                width = textBoxForApproxNodeBoundaries.Width * a + subgraph.DiameterOfOpenCollapseButton;
                height =
                    Math.Max(
                        textBoxForApproxNodeBoundaries.Height * subgraph.Label.FontSize / Label.DefaultFontSize,
                        subgraph.DiameterOfOpenCollapseButton);
            }

            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve(subgraph, width, height);
        }

        private void AssignLabelWidthHeight(Microsoft.Msagl.Core.Layout.ILabeledObject labeledGeomObj, DrawingObject drawingObj)
        {
            if (drawingObjectsToFrameworkElements.ContainsKey(drawingObj))
            {
                FrameworkElement fe = drawingObjectsToFrameworkElements[drawingObj];
                labeledGeomObj.Label.Width = fe.Width;
                labeledGeomObj.Label.Height = fe.Height;
            }
        }

        private ICurve GetNodeBoundaryCurve(Microsoft.Msagl.Drawing.Node node)
        {
            double width, height;

            FrameworkElement fe;
            if (drawingObjectsToFrameworkElements.TryGetValue(node, out fe))
            {
                width = fe.Width + 2 * node.Attr.LabelMargin;
                height = fe.Height + 2 * node.Attr.LabelMargin;
            }
            else
                return GetNodeBoundaryCurveByMeasuringText(node);

            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;
            return NodeBoundaryCurves.GetNodeBoundaryCurve(node, width, height);
        }

        private TextBlock textBoxForApproxNodeBoundaries;

        public static Size MeasureText(string text,
        FontFamily family, double size)
        {
            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(family, new System.Windows.FontStyle(), FontWeights.Regular, FontStretches.Normal),
                size,
                Brushes.Black,
                null);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private ICurve GetNodeBoundaryCurveByMeasuringText(Microsoft.Msagl.Drawing.Node node)
        {
            double width, height;
            if (String.IsNullOrEmpty(node.LabelText))
            {
                width = 10;
                height = 10;
            }
            else
            {
                var size = MeasureText(node.LabelText, new FontFamily(node.Label.FontName), node.Label.FontSize);
                width = size.Width;
                height = size.Height;
            }

            width += 2 * node.Attr.LabelMargin;
            height += 2 * node.Attr.LabelMargin;

            if (width < drawingGraph.Attr.MinNodeWidth)
                width = drawingGraph.Attr.MinNodeWidth;
            if (height < drawingGraph.Attr.MinNodeHeight)
                height = drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve(node, width, height);
        }

        private void SetUpTextBoxForApproxNodeBoundaries()
        {
            textBoxForApproxNodeBoundaries = new TextBlock
            {
                Text = "Fox jumping over River",
                FontFamily = new FontFamily(Label.DefaultFontName),
                FontSize = Label.DefaultFontSize,
            };

            textBoxForApproxNodeBoundaries.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBoxForApproxNodeBoundaries.Width = textBoxForApproxNodeBoundaries.DesiredSize.Width;
            textBoxForApproxNodeBoundaries.Height = textBoxForApproxNodeBoundaries.DesiredSize.Height;
        }

        private void CreateFrameworkElementsForLabelsOnly()
        {
            foreach (var edge in drawingGraph.Edges)
            {
                var fe = CreateDefaultFrameworkElementForDrawingObject(edge);
                if (fe != null)
                    if (GraphCanvas.Dispatcher.CheckAccess())
                        fe.Tag = new VLabel(edge, fe);
                    else
                    {
                        var localEdge = edge;
                        GraphCanvas.Dispatcher.Invoke(() => fe.Tag = new VLabel(localEdge, fe));
                    }
            }

            foreach (var node in drawingGraph.Nodes)
                CreateDefaultFrameworkElementForDrawingObject(node);
            if (drawingGraph.RootSubgraph != null)
                foreach (var subgraph in drawingGraph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf())
                    CreateDefaultFrameworkElementForDrawingObject(subgraph);
        }

        //        void CreateFrameworkElementForEdgeLabel(DrawingEdge edge) {
        //            var textBlock = CreateTextBlockForDrawingObj(edge);
        //            if (textBlock == null) return;
        //            drawingGraphObjectsToTextBoxes[edge] = textBlock;
        //            textBlock.Tag = new VLabel(edge, textBlock);
        //        }

        public void RegisterLabelCreator(DrawingObject drawingObject, Func<DrawingObject, FrameworkElement> func)
        {
            registeredCreators[drawingObject] = func;
        }

        public void UnregisterLabelCreator(DrawingObject drawingObject)
        {
            registeredCreators.Remove(drawingObject);
        }

        public Func<DrawingObject, FrameworkElement> GetLabelCreator(DrawingObject drawingObject)
        {
            return registeredCreators[drawingObject];
        }

        private FrameworkElement CreateTextBlockForDrawingObj(DrawingObject drawingObj)
        {
            Func<DrawingObject, FrameworkElement> registeredCreator;
            if (registeredCreators.TryGetValue(drawingObj, out registeredCreator))
                return registeredCreator(drawingObj);
            if (drawingObj is Subgraph)
                return null; //todo: add Label support later
            var labeledObj = drawingObj as ILabeledObject;
            if (labeledObj == null)
                return null;

            var drawingLabel = labeledObj.Label;
            if (drawingLabel == null)
                return null;

            TextBlock textBlock = null;
            if (GraphCanvas.Dispatcher.CheckAccess())
                textBlock = CreateTextBlock(drawingLabel);
            else
                GraphCanvas.Dispatcher.Invoke(() => textBlock = CreateTextBlock(drawingLabel));

            return textBlock;
        }

        private static TextBlock CreateTextBlock(Label drawingLabel)
        {
            var textBlock = new TextBlock
            {
                Tag = drawingLabel,
                Text = drawingLabel.Text,
                FontFamily = new FontFamily(drawingLabel.FontName),
                FontSize = drawingLabel.FontSize,
                Foreground = drawingLabel.FontColor.ToWpf()
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;
            return textBlock;
        }

        private FrameworkElement CreateDefaultFrameworkElementForDrawingObject(DrawingObject drawingObject)
        {
            lock (this)
            {
                var textBlock = CreateTextBlockForDrawingObj(drawingObject);
                if (textBlock != null)
                    drawingObjectsToFrameworkElements[drawingObject] = textBlock;
                return textBlock;
            }
        }

        public void DrawRubberLine(MsaglMouseEventArgs args)
        {
            DrawRubberLine(ScreenToSource(args));
        }

        public void StopDrawingRubberLine()
        {
            GraphCanvas.Children.Remove(_rubberLinePath);
            _rubberLinePath = null;
            GraphCanvas.Children.Remove(_targetArrowheadPathForRubberEdge);
            _targetArrowheadPathForRubberEdge = null;
        }

        public void AddEdge(IViewerEdge edge, bool registerForUndo)
        {
            //if (registerForUndo) drawingLayoutEditor.RegisterEdgeAdditionForUndo(edge);

            var drawingEdge = edge.Edge;
            Edge geomEdge = drawingEdge.GeometryEdge;

            drawingGraph.AddPrecalculatedEdge(drawingEdge);
            drawingGraph.GeometryGraph.Edges.Add(geomEdge);
        }

        public IViewerEdge CreateEdgeWithGivenGeometry(DrawingEdge drawingEdge)
        {
            return CreateEdge(drawingEdge, drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings);
        }

        public void AddNode(IViewerNode node, bool registerForUndo)
        {
            if (drawingGraph == null)
                throw new InvalidOperationException(); // adding a node when the graph does not exist
            var vNode = (VNode)node;
            drawingGraph.AddNode(vNode.Node);
            drawingGraph.GeometryGraph.Nodes.Add(vNode.Node.GeometryNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);
            GraphCanvas.Children.Add(vNode.FrameworkElementOfNodeForLabel);
            layoutEditor.CleanObstacles();
        }

        public IViewerObject AddNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            Graph.AddNode(drawingNode);
            var vNode = CreateVNode(drawingNode);
            LayoutEditor.AttachLayoutChangeEvent(vNode);
            LayoutEditor.CleanObstacles();
            return vNode;
        }

        public void RemoveEdge(IViewerEdge edge, bool registerForUndo)
        {
            lock (this)
            {
                var vedge = (VEdge)edge;
                var dedge = vedge.Edge;
                drawingGraph.RemoveEdge(dedge);
                drawingGraph.GeometryGraph.Edges.Remove(dedge.GeometryEdge);
                drawingObjectsToFrameworkElements.Remove(dedge);
                drawingObjectsToIViewerObjects.Remove(dedge);

                vedge.RemoveItselfFromCanvas(GraphCanvas);
            }
        }

        public void RemoveNode(IViewerNode node, bool registerForUndo)
        {
            lock (this)
            {
                RemoveEdges(node.Node.OutEdges);
                RemoveEdges(node.Node.InEdges);
                RemoveEdges(node.Node.SelfEdges);
                drawingObjectsToFrameworkElements.Remove(node.Node);
                drawingObjectsToIViewerObjects.Remove(node.Node);
                var vnode = (VNode)node;
                vnode.DetachFromCanvas(GraphCanvas);

                drawingGraph.RemoveNode(node.Node);
                drawingGraph.GeometryGraph.Nodes.Remove(node.Node.GeometryNode);
                layoutEditor.DetachNode(node);
                layoutEditor.CleanObstacles();
            }
        }

        private void RemoveEdges(IEnumerable<DrawingEdge> drawingEdges)
        {
            foreach (var de in drawingEdges.ToArray())
            {
                var vedge = (VEdge)drawingObjectsToIViewerObjects[de];
                RemoveEdge(vedge, false);
            }
        }

        public IViewerEdge RouteEdge(DrawingEdge drawingEdge)
        {
            var geomEdge = GeometryGraphCreator.CreateGeometryEdgeFromDrawingEdge(drawingEdge);
            var geomGraph = drawingGraph.GeometryGraph;
            LayoutHelpers.RouteAndLabelEdges(geomGraph, drawingGraph.LayoutAlgorithmSettings, new[] { geomEdge });
            return CreateEdge(drawingEdge, drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings);
        }

        public IViewerGraph ViewerGraph { get; set; }

        public double ArrowheadLength
        {
            get { return 0.2 * DpiX / CurrentScale; }
        }

        public void SetSourcePortForEdgeRouting(Point portLocation)
        {
            _sourcePortLocationForEdgeRouting = portLocation;
            if (_sourcePortCircle == null)
            {
                _sourcePortCircle = CreatePortPath();
                GraphCanvas.Children.Add(_sourcePortCircle);
            }
            _sourcePortCircle.Width = _sourcePortCircle.Height = UnderlyingPolylineCircleRadius;
            _sourcePortCircle.StrokeThickness = _sourcePortCircle.Width / 10;
            Wpf2MsaglConverters.PositionFrameworkElement(_sourcePortCircle, portLocation, 1);
        }

        private Ellipse CreatePortPath()
        {
            return new Ellipse
            {
                Stroke = Brushes.Brown,
                Fill = Brushes.Brown,
            };
        }

        public void SetTargetPortForEdgeRouting(Point portLocation)
        {
            if (TargetPortCircle == null)
            {
                TargetPortCircle = CreatePortPath();
                GraphCanvas.Children.Add(TargetPortCircle);
            }
            TargetPortCircle.Width = TargetPortCircle.Height = UnderlyingPolylineCircleRadius;
            TargetPortCircle.StrokeThickness = TargetPortCircle.Width / 10;
            Wpf2MsaglConverters.PositionFrameworkElement(TargetPortCircle, portLocation, 1);
        }

        public void RemoveSourcePortEdgeRouting()
        {
            GraphCanvas.Children.Remove(_sourcePortCircle);
            _sourcePortCircle = null;
        }

        public void RemoveTargetPortEdgeRouting()
        {
            GraphCanvas.Children.Remove(TargetPortCircle);
            TargetPortCircle = null;
        }

        public void DrawRubberEdge(EdgeGeometry edgeGeometry)
        {
            if (_rubberEdgePath == null)
            {
                _rubberEdgePath = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                GraphCanvas.Children.Add(_rubberEdgePath);
                _targetArrowheadPathForRubberEdge = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                GraphCanvas.Children.Add(_targetArrowheadPathForRubberEdge);
            }
            _rubberEdgePath.Data = VEdge.GetICurveWpfGeometry(edgeGeometry.Curve);
            _targetArrowheadPathForRubberEdge.Data = VEdge.DefiningTargetArrowHead(edgeGeometry,
                                                                                  edgeGeometry.LineWidth);
        }

        private bool UnderLayout
        {
            get { return _backgroundWorker != null; }
        }

        public void StopDrawingRubberEdge()
        {
            GraphCanvas.Children.Remove(_rubberEdgePath);
            GraphCanvas.Children.Remove(_targetArrowheadPathForRubberEdge);
            _rubberEdgePath = null;
            _targetArrowheadPathForRubberEdge = null;
        }

        public PlaneTransformation Transform
        {
            get
            {
                var mt = GraphCanvas.RenderTransform as MatrixTransform;
                if (mt == null)
                    return PlaneTransformation.UnitTransformation;
                var m = mt.Matrix;
                return new PlaneTransformation(m.M11, m.M12, m.OffsetX, m.M21, m.M22, m.OffsetY);
            }
            set
            {
                SetRenderTransformWithoutRaisingEvents(value);

                if (ViewChangeEvent != null)
                    ViewChangeEvent(null, null);
            }
        }

        private void SetRenderTransformWithoutRaisingEvents(PlaneTransformation value)
        {
            GraphCanvas.RenderTransform = new MatrixTransform(value[0, 0], value[0, 1], value[1, 0], value[1, 1],
                                                              value[0, 2],
                                                              value[1, 2]);
        }

        public bool NeedToCalculateLayout
        {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        /// <summary>
        /// the cancel token used to cancel a long running layout
        /// </summary>
        public CancelToken CancelToken
        {
            get { return _cancelToken; }
            set { _cancelToken = value; }
        }

        /// no layout is done, but the overlap is removed for graphs with geometry
        /// </summary>
        public bool NeedToRemoveOverlapOnly { get; set; }

        public void DrawRubberLine(Point rubberEnd)
        {
            if (_rubberLinePath == null)
            {
                _rubberLinePath = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                GraphCanvas.Children.Add(_rubberLinePath);
                //                targetArrowheadPathForRubberLine = new Path {
                //                    Stroke = Brushes.Black,
                //                    StrokeThickness = GetBorderPathThickness()*3
                //                };
                //                graphCanvas.Children.Add(targetArrowheadPathForRubberLine);
            }
            _rubberLinePath.Data =
                VEdge.GetICurveWpfGeometry(new LineSegment(_sourcePortLocationForEdgeRouting, rubberEnd));
        }

        public void StartDrawingRubberLine(Point startingPoint)
        {
        }

        #endregion IViewer stuff

        public IViewerNode CreateIViewerNode(Microsoft.Msagl.Drawing.Node drawingNode, Point center, object visualElement)
        {
            if (drawingGraph == null)
                return null;
            var frameworkElement = visualElement as FrameworkElement ?? CreateTextBlockForDrawingObj(drawingNode);
            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;
            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
            drawingNode.GeometryNode = new Node(bc, drawingNode) { Center = center };
            var vNode = CreateVNode(drawingNode);
            drawingGraph.AddNode(drawingNode);
            drawingGraph.GeometryGraph.Nodes.Add(drawingNode.GeometryNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);
            MakeRoomForNewNode(drawingNode);

            return vNode;
        }

        private void MakeRoomForNewNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            IncrementalDragger incrementalDragger = new IncrementalDragger(new[] { drawingNode.GeometryNode },
                                                                           Graph.GeometryGraph,
                                                                           Graph.LayoutAlgorithmSettings);
            incrementalDragger.Drag(new Point());

            foreach (var n in incrementalDragger.ChangedGraph.Nodes)
            {
                var dn = (Microsoft.Msagl.Drawing.Node)n.UserData;
                var vn = drawingObjectsToIViewerObjects[dn] as VNode;
                if (vn != null)
                    vn.Invalidate();
            }

            foreach (var n in incrementalDragger.ChangedGraph.Edges)
            {
                var dn = (Microsoft.Msagl.Drawing.Edge)n.UserData;
                var ve = drawingObjectsToIViewerObjects[dn] as VEdge;
                if (ve != null)
                    ve.Invalidate();
            }
        }
    }
}