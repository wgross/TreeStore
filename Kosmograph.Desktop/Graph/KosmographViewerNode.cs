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
        public Path BoundaryPath;
        public FrameworkElement FrameworkElementOfNodeForLabel;
        private readonly Func<Edge, VEdge> _funcFromDrawingEdgeToVEdge;
        private Subgraph _subgraph;
        private Node _node;
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

        public Node Node
        {
            get { return _node; }
            private set
            {
                _node = value;
                _subgraph = _node as Subgraph;
            }
        }

        public KosmographViewerNode(Node node, FrameworkElement frameworkElementOfNodeForLabelOfLabel,
            Func<Edge, VEdge> funcFromDrawingEdgeToVEdge, Func<double> pathStrokeThicknessFunc)
        {
            PathStrokeThicknessFunc = pathStrokeThicknessFunc;
            Node = node;
            FrameworkElementOfNodeForLabel = frameworkElementOfNodeForLabelOfLabel;

            _funcFromDrawingEdgeToVEdge = funcFromDrawingEdgeToVEdge;

            CreateNodeBoundaryPath();
            if (FrameworkElementOfNodeForLabel != null)
            {
                FrameworkElementOfNodeForLabel.Tag = this; //get a backpointer to the KosmographViewerNode
                Wpf2MsaglConverters.PositionFrameworkElement(FrameworkElementOfNodeForLabel, node.GeometryNode.Center, 1);
                Panel.SetZIndex(FrameworkElementOfNodeForLabel, Panel.GetZIndex(BoundaryPath) + 1);
            }
            SetupSubgraphDrawing();
            Node.Attr.VisualsChanged += (a, b) => Invalidate();
            Node.IsVisibleChanged += obj =>
            {
                foreach (var frameworkElement in FrameworkElements)
                {
                    frameworkElement.Visibility = Node.IsVisible ? Visibility.Visible : Visibility.Hidden;
                }
            };
        }

        public IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                if (FrameworkElementOfNodeForLabel != null) yield return FrameworkElementOfNodeForLabel;
                if (BoundaryPath != null) yield return BoundaryPath;
                if (_collapseButtonBorder != null)
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

            Panel.SetZIndex(_collapseButtonBorder, Panel.GetZIndex(BoundaryPath) + 1);

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

        /// <summary>
        /// </summary>
        public event Action<IViewerNode> IsCollapsedChanged;

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

        public void CreateNodeBoundaryPath()
        {
            if (FrameworkElementOfNodeForLabel != null)
            {
                // FrameworkElementOfNode.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var center = Node.GeometryNode.Center;
                var margin = 2 * Node.Attr.LabelMargin;
                var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(Node,
                    FrameworkElementOfNodeForLabel
                        .Width + margin,
                    FrameworkElementOfNodeForLabel
                        .Height + margin);
                bc.Translate(center);
            }
            BoundaryPath = new Path { Data = CreatePathFromNodeBoundary(), Tag = this };
            Panel.SetZIndex(BoundaryPath, ZIndex);
            SetFillAndStroke();
            if (Node.Label != null)
            {
                BoundaryPath.ToolTip = Node.LabelText;
                if (FrameworkElementOfNodeForLabel != null)
                    FrameworkElementOfNodeForLabel.ToolTip = Node.LabelText;
            }
        }

        public Func<double> PathStrokeThicknessFunc;

        private double PathStrokeThickness
        {
            get { return PathStrokeThicknessFunc != null ? PathStrokeThicknessFunc() : Node.Attr.LineWidth; }
        }

        private byte GetTransparency(byte t)
        {
            return t;
        }

        private void SetFillAndStroke()
        {
            byte trasparency = GetTransparency(Node.Attr.Color.A);
            BoundaryPath.Stroke = new Microsoft.Msagl.Drawing.Color(trasparency, Node.Attr.Color.R, Node.Attr.Color.G, Node.Attr.Color.B).ToWpf();
            SetBoundaryFill();
            BoundaryPath.StrokeThickness = PathStrokeThickness;

            var textBlock = FrameworkElementOfNodeForLabel as TextBlock;
            if (textBlock != null)
            {
                var col = Node.Label.FontColor;
                textBlock.Foreground = new Microsoft.Msagl.Drawing.Color(GetTransparency(col.A), col.R, col.G, col.B).ToWpf();
            }
        }

        private void SetBoundaryFill()
        {
            BoundaryPath.Fill = Node.Attr.FillColor.ToWpf();
        }

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

        private Geometry CreatePathFromNodeBoundary()
        {
            Geometry geometry;
            switch (Node.Attr.Shape)
            {
                case Shape.Box:
                case Shape.House:
                case Shape.InvHouse:
                case Shape.Diamond:
                case Shape.Octagon:
                case Shape.Hexagon:

                    geometry = CreateGeometryFromMsaglCurve(Node.GeometryNode.BoundaryCurve);
                    break;

                case Shape.DoubleCircle:
                    geometry = DoubleCircle();
                    break;

                default:
                    geometry = GetEllipseGeometry();
                    break;
            }

            return geometry;
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

        #region Implementation of IViewerObject

        public DrawingObject DrawingObject
        {
            get { return Node; }
        }

        private bool markedForDragging;

        /// <summary>
        /// Implements a property of an interface IEditViewer
        /// </summary>
        public bool MarkedForDragging
        {
            get
            {
                return markedForDragging;
            }
            set
            {
                markedForDragging = value;
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

        #endregion Implementation of IViewerObject

        public IEnumerable<IViewerEdge> InEdges
        {
            get { return Node.InEdges.Select(e => _funcFromDrawingEdgeToVEdge(e)); }
        }

        public IEnumerable<IViewerEdge> OutEdges
        {
            get { return Node.OutEdges.Select(e => _funcFromDrawingEdgeToVEdge(e)); }
        }

        public IEnumerable<IViewerEdge> SelfEdges
        {
            get { return Node.SelfEdges.Select(e => _funcFromDrawingEdgeToVEdge(e)); }
        }

        public void Invalidate()
        {
            if (!Node.IsVisible)
            {
                foreach (var fe in FrameworkElements)
                    fe.Visibility = Visibility.Hidden;
                return;
            }

            BoundaryPath.Data = CreatePathFromNodeBoundary();

            Wpf2MsaglConverters.PositionFrameworkElement(FrameworkElementOfNodeForLabel, Node.BoundingBox.Center, 1);

            SetFillAndStroke();
            if (_subgraph == null) return;
            PositionTopMarginBorder((Cluster)_subgraph.GeometryNode);
            double collapseBorderSize = GetCollapseBorderSymbolSize();
            var collapseButtonCenter = GetCollapseButtonCenter(collapseBorderSize);
            Wpf2MsaglConverters.PositionFrameworkElement(_collapseButtonBorder, collapseButtonCenter, 1);
            double w = collapseBorderSize * 0.4;
            _collapseSymbolPath.Data = CreateCollapseSymbolPath(collapseButtonCenter + new Point(0, -w / 2), w);
            _collapseSymbolPath.RenderTransform = ((Cluster)_subgraph.GeometryNode).IsCollapsed
                ? new RotateTransform(180, collapseButtonCenter.X,
                    collapseButtonCenter.Y)
                : null;

            _topMarginRect.Visibility =
                _collapseSymbolPath.Visibility =
                    _collapseButtonBorder.Visibility = Visibility.Visible;
        }

        public override string ToString()
        {
            return Node.Id;
        }

        public void DetachFromCanvas(Canvas graphCanvas)
        {
            if (BoundaryPath != null)
                graphCanvas.Children.Remove(BoundaryPath);
            if (FrameworkElementOfNodeForLabel != null)
                graphCanvas.Children.Remove(FrameworkElementOfNodeForLabel);
        }
    }
}