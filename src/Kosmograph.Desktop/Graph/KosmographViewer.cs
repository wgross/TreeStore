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

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Miscellaneous;
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
using Label = Microsoft.Msagl.Drawing.Label;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using ModifierKeys = Microsoft.Msagl.Drawing.ModifierKeys;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using Rectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using Size = System.Windows.Size;
using WpfPoint = System.Windows.Point;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer : IViewer
    {
        private readonly object syncRoot = new object();

        private Path _targetArrowheadPathForRubberEdge;

        private Path _rubberEdgePath;
        private Path _rubberLinePath;
        private Point _sourcePortLocationForEdgeRouting;

        //WpfPoint _objectUnderMouseDetectionLocation;

        private BackgroundWorker backgroundWorker;
        private Point mouseDownPositionInGraph;
        private bool mouseDownPositionInGraph_initialized;

        private Ellipse _sourcePortCircle;
        protected Ellipse TargetPortCircle { get; set; }

        private WpfPoint _objectUnderMouseDetectionLocation;

        /// <summary>
        /// if set to true will layout in a task
        /// </summary>

        private readonly LayoutEditor layoutEditor;

        /*
                Thread layoutThread;
        */
        private bool needToCalculateLayout = true;

        private static double _dpiX;
        private static int _dpiY;

        public KosmographViewer(Canvas graphCanvas)
        {
            this.layoutEditor = new LayoutEditor(this);
            this.ViewChangeEvent += AdjustBtrectRenderTransform;

            this.GraphCanvas = graphCanvas;
            this.GraphCanvas.SizeChanged += GraphCanvasSizeChanged;
            this.GraphCanvas.MouseLeftButtonDown += GraphCanvasMouseLeftButtonDown;
            this.GraphCanvas.MouseRightButtonDown += GraphCanvasRightMouseDown;
            this.GraphCanvas.MouseMove += GraphCanvasMouseMove;
            this.GraphCanvas.MouseLeftButtonUp += GraphCanvasMouseLeftButtonUp;
            this.GraphCanvas.MouseWheel += GraphCanvasMouseWheel;
            this.GraphCanvas.MouseRightButtonUp += GraphCanvasRightMouseUp;

            //LargeGraphNodeCountThreshold = 0;
            this.LayoutEditingEnabled = true;

            this.clickCounter = new ClickCounter(() => Mouse.GetPosition((IInputElement)this.GraphCanvas.Parent));
            this.clickCounter.Elapsed += ClickCounterElapsed;

            this.RunLayoutAsync = false;
        }

        #region WPF stuff

        /// <summary>
        /// adds the main panel of the viewer to the children of the parent
        /// </summary>
        /// <param name="panel"></param>
        //public void BindToPanel(Panel panel)
        //{
        //    panel.Children.Add(GraphCanvas);
        //    GraphCanvas.UpdateLayout();
        //}

        private void ClickCounterElapsed(object sender, EventArgs e)
        {
            var vedge = this.clickCounter.ClickedObject as KosmographViewerEdge;
            if (vedge != null)
            {
                if (this.clickCounter.UpCount == this.clickCounter.DownCount && this.clickCounter.UpCount == 1)
                    this.HandleClickForEdge(vedge);
            }
            this.clickCounter.ClickedObject = null;
        }

        private void AdjustBtrectRenderTransform(object sender, EventArgs e)
        {
            if (canvasBackgroundRect == null)
                return;
            canvasBackgroundRect.RenderTransform = (Transform)GraphCanvas.RenderTransform.Inverse;
            var parent = (Panel)GraphCanvas.Parent;
            canvasBackgroundRect.Width = parent.ActualWidth;
            canvasBackgroundRect.Height = parent.ActualHeight;
        }

        private void HandleClickForEdge(KosmographViewerEdge vEdge)
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

            var vNode = viewerObject as KosmographViewerNode;
            if (vNode != null) ret = (FrameworkElement)(vNode.NodeLabel) ?? (FrameworkElement)(vNode.NodeBoundaryPath);
            else
            {
                var vLabel = viewerObject as KosmographViewerEdgeLabel;
                if (vLabel != null) ret = vLabel.EdgeLabelVisual;
                else
                {
                    var vEdge = viewerObject as KosmographViewerEdge;
                    if (vEdge != null) ret = vEdge.EdgePath;
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
                        if (tag is KosmographViewerNode || tag is Label)
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
                    var edge = viewerObject as KosmographViewerEdge;
                    if (edge != null)
                        if (edge.EdgeLabelViewer != null)
                            yield return edge.EdgeLabelViewer;
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

        private readonly ClickCounter clickCounter;
        public string MsaglFileToSave;

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
                    ((KosmographViewerNode)o).Invalidate();
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

   
        private TextBlock textBoxForApproxNodeBoundaries;

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

        //        void CreateFrameworkElementForEdgeLabel(DrawingEdge edge) {
        //            var textBlock = CreateTextBlockForDrawingObj(edge);
        //            if (textBlock == null) return;
        //            drawingGraphObjectsToTextBoxes[edge] = textBlock;
        //            textBlock.Tag = new VLabel(edge, textBlock);
        //        }

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
            return CreateEdgeViewer(drawingEdge, drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings);
        }

        public void AddNode(IViewerNode node, bool registerForUndo)
        {
            if (drawingGraph == null)
                throw new InvalidOperationException(); // adding a node when the graph does not exist
            var vNode = (KosmographViewerNode)node;
            drawingGraph.AddNode(vNode.Node);
            drawingGraph.GeometryGraph.Nodes.Add(vNode.Node.GeometryNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);
            GraphCanvas.Children.Add(vNode.NodeLabel);
            layoutEditor.CleanObstacles();
        }

        public IViewerObject AddNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            Graph.AddNode(drawingNode);
            var vNode = GetOrCreateViewerNode(drawingNode);
            LayoutEditor.AttachLayoutChangeEvent(vNode);
            LayoutEditor.CleanObstacles();
            return vNode;
        }

        public void RemoveEdge(IViewerEdge edge, bool registerForUndo)
        {
            lock (this.syncRoot)
            {
                var vedge = (KosmographViewerEdge)edge;
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
            lock (this.syncRoot)
            {
                RemoveEdges(node.Node.OutEdges);
                RemoveEdges(node.Node.InEdges);
                RemoveEdges(node.Node.SelfEdges);
                drawingObjectsToFrameworkElements.Remove(node.Node);
                drawingObjectsToIViewerObjects.Remove(node.Node);
                var vnode = (KosmographViewerNode)node;
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
                var vedge = (KosmographViewerEdge)drawingObjectsToIViewerObjects[de];
                RemoveEdge(vedge, false);
            }
        }

        public IViewerEdge RouteEdge(DrawingEdge drawingEdge)
        {
            var geomEdge = GeometryGraphCreator.CreateGeometryEdgeFromDrawingEdge(drawingEdge);
            var geomGraph = drawingGraph.GeometryGraph;
            LayoutHelpers.RouteAndLabelEdges(geomGraph, drawingGraph.LayoutAlgorithmSettings, new[] { geomEdge });
            return CreateEdgeViewer(drawingEdge, drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings);
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
            _rubberEdgePath.Data = VisualsFactory.CreateEdgePath(edgeGeometry.Curve);
            _targetArrowheadPathForRubberEdge.Data = VisualsFactory.CreateEdgeTargetArrow(edgeGeometry, edgeGeometry.LineWidth);
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

        /// <summary>
        /// the cancel token used to cancel a long running layout
        /// </summary>

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
            _rubberLinePath.Data = VisualsFactory.CreateEdgePath(new LineSegment(_sourcePortLocationForEdgeRouting, rubberEnd));
        }

        public void StartDrawingRubberLine(Point startingPoint)
        {
        }

        #endregion IViewer stuff
    }
}