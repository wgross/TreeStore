using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Ellipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using Polyline = Microsoft.Msagl.Core.Geometry.Curves.Polyline;
using Shape = Microsoft.Msagl.Drawing.Shape;
using Size = System.Windows.Size;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerNode : IViewerNode, IInvalidatable
    {
        public Path NodeBoundaryPath { get; private set; }
        public FrameworkElement NodeLabelFrameworkElement { get; }
        private readonly Func<Edge, VEdge> _funcFromDrawingEdgeToVEdge;

        private Border _collapseButtonBorder;
        private Rectangle _topMarginRect;
        private Path _collapseSymbolPath;
        private readonly Brush _collapseSymbolPathInactive = Brushes.Silver;

        public int ZIndex
        {
            get
            {
                var geomNode = Node.GeometryNode;
                if (geomNode == null)
                    return 0;
                int ret = 0;
                do
                {
                    if (geomNode.ClusterParents == null)
                        return ret;
                    geomNode = geomNode.ClusterParents.FirstOrDefault();
                    if (geomNode != null)
                        ret++;
                    else
                        return ret;
                } while (true);
            }
        }

        public KosmographViewerNode(Node node, FrameworkElement nodeLabelFrameworkElement,
            Func<Edge, VEdge> funcFromDrawingEdgeToVEdge, Func<double> pathStrokeThicknessFunc)
        {
            this.PathStrokeThicknessFunc = pathStrokeThicknessFunc;
            this.Node = node;
            this.NodeLabelFrameworkElement = nodeLabelFrameworkElement;

            _funcFromDrawingEdgeToVEdge = funcFromDrawingEdgeToVEdge;

            this.NodeBoundaryPath = this.CreateNodeBoundaryPath(this.NodeLabelFrameworkElement);

            if (this.NodeLabelFrameworkElement != null)
            {
                this.NodeLabelFrameworkElement.Tag = this; //get a backpointer to the KosmographViewerNode
                Wpf2MsaglConverters.PositionFrameworkElement(NodeLabelFrameworkElement, node.GeometryNode.Center, 1);
                Panel.SetZIndex(NodeLabelFrameworkElement, Panel.GetZIndex(NodeBoundaryPath) + 1);
            }

            this.SetupSubgraphDrawing();

            Node.Attr.VisualsChanged += (a, b) => Invalidate();
            Node.IsVisibleChanged += obj =>
            {
                foreach (var frameworkElement in this.FrameworkElements)
                {
                    frameworkElement.Visibility = Node.IsVisible ? Visibility.Visible : Visibility.Hidden;
                }
            };
        }

        #region IViewerObject Members

        public DrawingObject DrawingObject => this.Node;

        private bool markedForDragging;

        /// <summary>
        /// Implements a property of an interface IEditViewer
        /// </summary>
        public bool MarkedForDragging
        {
            get => this.markedForDragging;
            set
            {
                this.markedForDragging = value;
                if (value)
                {
                    MarkedForDraggingEvent?.Invoke(this, null);
                }
                else
                {
                    UnmarkedForDraggingEvent?.Invoke(this, null);
                }
            }
        }

        public event EventHandler MarkedForDraggingEvent;

        public event EventHandler UnmarkedForDraggingEvent;

        #endregion IViewerObject Members

        #region IViewerNode Members

        public Node Node
        {
            get { return _node; }
            private set
            {
                _node = value;
                _subgraph = _node as Subgraph;
            }
        }

        private Subgraph _subgraph;
        private Node _node;

        public IEnumerable<IViewerEdge> InEdges => this.Node.InEdges.Select(e => _funcFromDrawingEdgeToVEdge(e));

        public IEnumerable<IViewerEdge> OutEdges => this.Node.OutEdges.Select(e => _funcFromDrawingEdgeToVEdge(e));

        public IEnumerable<IViewerEdge> SelfEdges => this.Node.SelfEdges.Select(e => _funcFromDrawingEdgeToVEdge(e));

        public event Action<IViewerNode> IsCollapsedChanged;

        #endregion IViewerNode Members

        #region IInvalidatable members

        public void Invalidate()
        {
            if (!this.Node.IsVisible)
            {
                foreach (var fe in FrameworkElements)
                    fe.Visibility = Visibility.Hidden;
                return;
            }

            this.NodeBoundaryPath.Data = this.CreatePathFromNodeBoundary(this.Node.Attr.Shape);

            Wpf2MsaglConverters.PositionFrameworkElement(NodeLabelFrameworkElement, Node.BoundingBox.Center, 1);

            this.SetFillAndStroke(this.NodeBoundaryPath);

            if (_subgraph is null)
                return;

            this.PositionTopMarginBorder((Cluster)_subgraph.GeometryNode);

            double collapseBorderSize = GetCollapseBorderSymbolSize();
            var collapseButtonCenter = GetCollapseButtonCenter(collapseBorderSize);

            Wpf2MsaglConverters.PositionFrameworkElement(_collapseButtonBorder, collapseButtonCenter, 1);

            double w = collapseBorderSize * 0.4;

            _collapseSymbolPath.Data = CreateCollapseSymbolPath(collapseButtonCenter + new Point(0, -w / 2), w);
            _collapseSymbolPath.RenderTransform = ((Cluster)_subgraph.GeometryNode).IsCollapsed
                ? new RotateTransform(180, collapseButtonCenter.X, collapseButtonCenter.Y)
                : null;

            _topMarginRect.Visibility = _collapseSymbolPath.Visibility = _collapseButtonBorder.Visibility = Visibility.Visible;
        }

        #endregion IInvalidatable members

        public IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                if (this.NodeLabelFrameworkElement != null)
                    yield return this.NodeLabelFrameworkElement;

                if (this.NodeBoundaryPath != null)
                    yield return NodeBoundaryPath;

                if (this._collapseButtonBorder != null)
                {
                    yield return _collapseButtonBorder;
                    yield return _topMarginRect;
                    yield return _collapseSymbolPath;
                }
            }
        }

        private void SetupSubgraphDrawing()
        {
            if (_subgraph == null) return;

            SetupTopMarginBorder();
            SetupCollapseSymbol();
        }

        private void SetupTopMarginBorder()
        {
            var cluster = (Cluster)_subgraph.GeometryObject;
            _topMarginRect = new Rectangle
            {
                Fill = Brushes.Transparent,
                Width = Node.Width,
                Height = cluster.RectangularBoundary.TopMargin
            };
            PositionTopMarginBorder(cluster);
            SetZIndexAndMouseInteractionsForTopMarginRect();
        }

        private void PositionTopMarginBorder(Cluster cluster)
        {
            var box = cluster.BoundaryCurve.BoundingBox;

            Wpf2MsaglConverters.PositionFrameworkElement(_topMarginRect,
                box.LeftTop + new Point(_topMarginRect.Width / 2, -_topMarginRect.Height / 2), 1);
        }

        private void SetZIndexAndMouseInteractionsForTopMarginRect()
        {
            _topMarginRect.MouseEnter += (a, b) =>
            {
                _collapseButtonBorder.Background = _subgraph.CollapseButtonColorActive.ToWpf();
                _collapseSymbolPath.Stroke = Brushes.Black;
            };

            _topMarginRect.MouseLeave += (a, b) =>
            {
                _collapseButtonBorder.Background = _subgraph.CollapseButtonColorInactive.ToWpf();
                _collapseSymbolPath.Stroke = Brushes.Silver;
            };

            Panel.SetZIndex(_topMarginRect, int.MaxValue);
        }

        private void SetupCollapseSymbol()
        {
            var collapseBorderSize = GetCollapseBorderSymbolSize();
            Debug.Assert(collapseBorderSize > 0);
            _collapseButtonBorder = new Border
            {
                Background = _subgraph.CollapseButtonColorInactive.ToWpf(),
                Width = collapseBorderSize,
                Height = collapseBorderSize,
                CornerRadius = new CornerRadius(collapseBorderSize / 2)
            };

            Panel.SetZIndex(_collapseButtonBorder, Panel.GetZIndex(NodeBoundaryPath) + 1);

            var collapseButtonCenter = GetCollapseButtonCenter(collapseBorderSize);
            Wpf2MsaglConverters.PositionFrameworkElement(_collapseButtonBorder, collapseButtonCenter, 1);

            double w = collapseBorderSize * 0.4;
            _collapseSymbolPath = new Path
            {
                Data = CreateCollapseSymbolPath(collapseButtonCenter + new Point(0, -w / 2), w),
                Stroke = _collapseSymbolPathInactive,
                StrokeThickness = 1
            };

            Panel.SetZIndex(_collapseSymbolPath, Panel.GetZIndex(_collapseButtonBorder) + 1);
            _topMarginRect.MouseLeftButtonDown += TopMarginRectMouseLeftButtonDown;
        }

        private void InvokeIsCollapsedChanged()
        {
            if (IsCollapsedChanged != null)
                IsCollapsedChanged(this);
        }

        private void TopMarginRectMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_collapseButtonBorder);
            if (pos.X <= _collapseButtonBorder.Width && pos.Y <= _collapseButtonBorder.Height && pos.X >= 0 &&
                pos.Y >= 0)
            {
                e.Handled = true;
                var cluster = (Cluster)_subgraph.GeometryNode;
                cluster.IsCollapsed = !cluster.IsCollapsed;
                InvokeIsCollapsedChanged();
            }
        }

        private double GetCollapseBorderSymbolSize()
        {
            return ((Cluster)_subgraph.GeometryNode).RectangularBoundary.TopMargin -
                   PathStrokeThickness / 2 - 0.5;
        }

        private Point GetCollapseButtonCenter(double collapseBorderSize)
        {
            var box = _subgraph.GeometryNode.BoundaryCurve.BoundingBox;
            //cannot trust subgraph.GeometryNode.BoundingBox for a cluster
            double offsetFromBoundaryPath = PathStrokeThickness / 2 + 0.5;
            var collapseButtonCenter = box.LeftTop + new Point(collapseBorderSize / 2 + offsetFromBoundaryPath,
                -collapseBorderSize / 2 - offsetFromBoundaryPath);
            return collapseButtonCenter;
        }

        /*
                void FlipCollapsePath() {
                    var size = GetCollapseBorderSymbolSize();
                    var center = GetCollapseButtonCenter(size);

                    if (collapsePathFlipped) {
                        collapsePathFlipped = false;
                        collapseSymbolPath.RenderTransform = null;
                    }
                    else {
                        collapsePathFlipped = true;
                        collapseSymbolPath.RenderTransform = new RotateTransform(180, center.X, center.Y);
                    }
                }
        */

        private Geometry CreateCollapseSymbolPath(Point center, double width)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = (center + new Point(-width, width)).ToWpf()
            };

            pathFigure.Segments.Add(new System.Windows.Media.LineSegment(center.ToWpf(), true));
            pathFigure.Segments.Add(new System.Windows.Media.LineSegment((center + new Point(width, width)).ToWpf(), true));

            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        #region Create NodeBoundaryPath

        public Func<double> PathStrokeThicknessFunc { get; }

        private double PathStrokeThickness
        {
            get { return PathStrokeThicknessFunc != null ? PathStrokeThicknessFunc() : Node.Attr.LineWidth; }
        }

        public Path CreateNodeBoundaryPath(FrameworkElement frameworkElementToDecorate)
        {
            if (frameworkElementToDecorate != null)
            {
                // FrameworkElementOfNode.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var margin = 2 * this.Node.Attr.LabelMargin;

                // MSAGL calculates the boundray curve.
                // it used the externally given width and height for this instead of the Nodes geometry data

                var boundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(this.Node, frameworkElementToDecorate.Width + margin, frameworkElementToDecorate.Height + margin);

                boundaryCurve.Translate(this.Node.GeometryNode.Center);
            }

            var nodeBoundaryPath = new Path
            {
                Data = this.CreatePathFromNodeBoundary(this.Node.Attr.Shape),
                Tag = this
            };

            Panel.SetZIndex(nodeBoundaryPath, this.ZIndex);

            this.SetFillAndStroke(nodeBoundaryPath);

            if (Node.Label != null)
            {
                nodeBoundaryPath.ToolTip = Node.LabelText;
                if (this.NodeLabelFrameworkElement != null)
                    this.NodeLabelFrameworkElement.ToolTip = Node.LabelText;
            }

            return nodeBoundaryPath;
        }

        private void SetFillAndStroke(Path nodeBoundaryPath)
        {
            byte transparency = this.Node.Attr.Color.A;

            nodeBoundaryPath.Stroke = this.Node.Attr.Color.ToWpf();
            nodeBoundaryPath.Fill = Node.Attr.FillColor.ToWpf();
            nodeBoundaryPath.StrokeThickness = this.PathStrokeThickness;

            var textBlock = this.NodeLabelFrameworkElement as TextBlock;
            if (textBlock != null)
            {
                var col = Node.Label.FontColor;
                textBlock.Foreground = this.Node.Attr.Color.ToWpf();
            }
        }

        private Geometry CreatePathFromNodeBoundary(Microsoft.Msagl.Drawing.Shape nodeShape)
        {
            switch (nodeShape)
            {
                case Shape.Box:
                case Shape.House:
                case Shape.InvHouse:
                case Shape.Diamond:
                case Shape.Octagon:
                case Shape.Hexagon:
                    return CreateGeometryFromMsaglCurve(Node.GeometryNode.BoundaryCurve);

                case Shape.DoubleCircle:
                    return DoubleCircle();

                default:
                    return GetEllipseGeometry();
            }
        }

        #endregion Create NodeBoundaryPath

        private Geometry DoubleCircle()
        {
            var box = Node.BoundingBox;
            double w = box.Width;
            double h = box.Height;
            var pathGeometry = new PathGeometry();
            var r = new Rect(box.Left, box.Bottom, w, h);
            pathGeometry.AddGeometry(new EllipseGeometry(r));
            var inflation = Math.Min(5.0, Math.Min(w / 3, h / 3));
            r.Inflate(-inflation, -inflation);
            pathGeometry.AddGeometry(new EllipseGeometry(r));
            return pathGeometry;
        }

        private Geometry CreateGeometryFromMsaglCurve(ICurve iCurve)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                IsClosed = true,
                IsFilled = true,
                StartPoint = iCurve.Start.ToWpf()
            };

            var curve = iCurve as Curve;
            if (curve != null)
            {
                AddCurve(pathFigure, curve);
            }
            else
            {
                var rect = iCurve as RoundedRect;
                if (rect != null)
                    AddCurve(pathFigure, rect.Curve);
                else
                {
                    var ellipse = iCurve as Ellipse;
                    if (ellipse != null)
                    {
                        return new EllipseGeometry(ellipse.Center.ToWpf(), ellipse.AxisA.Length, ellipse.AxisB.Length);
                    }
                    var poly = iCurve as Polyline;
                    if (poly != null)
                    {
                        var p = poly.StartPoint.Next;
                        do
                        {
                            pathFigure.Segments.Add(new System.Windows.Media.LineSegment(p.Point.ToWpf(), true));

                            p = p.NextOnPolyline;
                        } while (p != poly.StartPoint);
                    }
                }
            }

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private static void AddCurve(PathFigure pathFigure, Curve curve)
        {
            foreach (ICurve seg in curve.Segments)
            {
                var ls = seg as LineSegment;
                if (ls != null)
                    pathFigure.Segments.Add(new System.Windows.Media.LineSegment(ls.End.ToWpf(), true));
                else
                {
                    var ellipse = seg as Ellipse;
                    if (ellipse != null)
                        pathFigure.Segments.Add(new ArcSegment(ellipse.End.ToWpf(),
                            new Size(ellipse.AxisA.Length, ellipse.AxisB.Length),
                            Point.Angle(new Point(1, 0), ellipse.AxisA),
                            ellipse.ParEnd - ellipse.ParEnd >= Math.PI,
                            !ellipse.OrientedCounterclockwise()
                                ? SweepDirection.Counterclockwise
                                : SweepDirection.Clockwise, true));
                }
            }
        }

        private Geometry GetEllipseGeometry()
        {
            return new EllipseGeometry(Node.BoundingBox.Center.ToWpf(), Node.BoundingBox.Width / 2, Node.BoundingBox.Height / 2);
        }

        public override string ToString()
        {
            return Node.Id;
        }

        public void DetachFromCanvas(Canvas graphCanvas)
        {
            if (NodeBoundaryPath != null)
                graphCanvas.Children.Remove(NodeBoundaryPath);
            if (NodeLabelFrameworkElement != null)
                graphCanvas.Children.Remove(NodeLabelFrameworkElement);
        }
    }
}