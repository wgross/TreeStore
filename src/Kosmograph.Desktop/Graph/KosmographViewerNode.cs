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
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerNode : IViewerNode, IInvalidatable
    {
        public TextBlock NodeLabel { get; }
        private readonly Func<Edge, KosmographviewerEdge> funcFromDrawingEdgeToVEdge;

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

        public KosmographViewerNode(Node node, TextBlock nodeLabelFrameworkElement, Func<Edge, KosmographviewerEdge> funcFromDrawingEdgeToVEdge, Func<double> pathStrokeThicknessFunc)
        {
            this.pathStrokeThicknessFunc = pathStrokeThicknessFunc;
            this.funcFromDrawingEdgeToVEdge = funcFromDrawingEdgeToVEdge;

            this.Node = node;
            this.NodeLabel = nodeLabelFrameworkElement;
            this.NodeLabel.Tag = this; //get a backpointer to the KosmographViewerNode
            this.NodeBoundaryPath = new Path { Tag = this };
            this.UpdateNodeVisualsPosition();

            this.SetupSubgraphDrawing();

            this.Node.Attr.VisualsChanged += (a, b) => this.Invalidate();
            this.Node.IsVisibleChanged += obj =>
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

        public IEnumerable<IViewerEdge> InEdges => this.Node.InEdges.Select(e => funcFromDrawingEdgeToVEdge(e));

        public IEnumerable<IViewerEdge> OutEdges => this.Node.OutEdges.Select(e => funcFromDrawingEdgeToVEdge(e));

        public IEnumerable<IViewerEdge> SelfEdges => this.Node.SelfEdges.Select(e => funcFromDrawingEdgeToVEdge(e));

        public event Action<IViewerNode> IsCollapsedChanged;

        #endregion IViewerNode Members

        #region IInvalidatable members

        /// <summary>
        /// Handles any change in the visulas triggred by the MSAGl node classes.
        /// This doesn ot change the visuals itself to avoid endless recursion
        /// </summary>
        public void Invalidate()
        {
            Debug.Assert(this.NodeLabel.Dispatcher.CheckAccess());

            this.UpdateNodeVisuals();

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
                if (this.NodeLabel != null)
                    yield return this.NodeLabel;

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

        #region Nodes have a boundary

        public Path NodeBoundaryPath { get; set; }

        private Func<double> pathStrokeThicknessFunc { get; }

        private double PathStrokeThickness
        {
            get { return pathStrokeThicknessFunc != null ? this.pathStrokeThicknessFunc() : Node.Attr.LineWidth; }
        }

        private void UpdateNodeVisuals()
        {
            if (!this.Node.IsVisible)
            {
                foreach (var nodeVisuals in this.FrameworkElements)
                    nodeVisuals.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                this.NodeLabel.UpdateFrom(this.Node);
                this.NodeBoundaryPath.Update(this.Node);
                this.NodeBoundaryPath.StrokeThickness = this.PathStrokeThickness;
                this.UpdateNodeVisualsPosition();
            }
        }

        private void UpdateNodeVisualsPosition()
        {
            Wpf2MsaglConverters.PositionFrameworkElement(this.NodeLabel, this.Node.GeometryNode.Center, 1);
            Panel.SetZIndex(this.NodeBoundaryPath, this.ZIndex);
            Panel.SetZIndex(this.NodeLabel, this.ZIndex + 1);
        }

        #endregion Nodes have a boundary

        public override string ToString()
        {
            return Node.Id;
        }

        public void DetachFromCanvas(Canvas graphCanvas)
        {
            // i think the subgraph items are missing
            // --wgross 26.09.2018
            if (NodeBoundaryPath != null)
                graphCanvas.Children.Remove(NodeBoundaryPath);
            if (NodeLabel != null)
                graphCanvas.Children.Remove(NodeLabel);
        }
    }
}