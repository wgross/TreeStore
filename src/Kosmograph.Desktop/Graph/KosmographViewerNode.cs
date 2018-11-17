using Kosmograph.Desktop.Graph.Base;
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
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Point = Microsoft.Msagl.Core.Geometry.Point;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerNode : KosmographViewerItemBase, IViewerNode, IInvalidatable
    {
        #region Viewer node is associated to a DrawingNode from MSAGL graph

        public DrawingNode Node
        {
            get { return _node; }
            private set
            {
                _node = value;
                _subgraph = _node as Subgraph;
            }
        }

        private Subgraph _subgraph;

        private DrawingNode _node;

        #endregion Viewer node is associated to a DrawingNode from MSAGL graph

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

        private readonly Func<Edge, KosmographViewerEdge> funcFromDrawingEdgeToVEdge;

        private readonly Brush _collapseSymbolPathInactive = Brushes.Silver;

        #region Creation and Initialization of this instance

        public KosmographViewerNode(DrawingNode node, TextBlock nodeLabelTextBlock, Func<Edge, KosmographViewerEdge> funcFromDrawingEdgeToVEdge)
        {
            this.funcFromDrawingEdgeToVEdge = funcFromDrawingEdgeToVEdge;

            this.Node = node;
            this.NodeLabel = nodeLabelTextBlock;
            this.NodeBoundaryPath = new Path { Tag = this };
            this.UpdateNodeViewerVisuals();

            this.SetupSubgraphDrawing();

            this.Node.Attr.VisualsChanged += (a, b) => this.Invalidate();
            this.Node.IsVisibleChanged += this.UpdateVisibility;
        }

        #endregion Creation and Initialization of this instance

        #region IViewerObject Members

        public DrawingObject DrawingObject => this.Node;

        #endregion IViewerObject Members

        #region IViewerNode Members

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
            this.NodeLabel.InvokeInUiThread(() =>
            {
                this.UpdateNodeViewerVisuals();

                if (_subgraph is null)
                    return;

                this.UpdateSubGraphVisuals();
            });
        }

        #endregion IInvalidatable members

        #region Node Viewer Visuals

        private double PathStrokeThickness => this.PathStrokeThicknessFunc is null ? this.Node.Attr.LineWidth : this.PathStrokeThicknessFunc();

        public Func<double> PathStrokeThicknessFunc { set; private get; }

        /// <summary>
        /// The label text of the node visulizes the <see cref="Microsoft.Msagl.Drawing.Label"/> of <see cref="Node"/>.
        /// </summary>
        public TextBlock NodeLabel { get; }

        /// <summary>
        /// The path gemeotry surrunding the <see cref="NodeLabel"/> iof the viewers <see cref="Node"/>
        /// </summary>
        public Path NodeBoundaryPath { get; }

        // subgraph visuals

        private Border _collapseButtonBorder;
        private Rectangle _topMarginRect;
        private Path _collapseSymbolPath;

        override public IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                if (this.NodeLabel != null)
                    yield return this.NodeLabel;

                if (this.NodeBoundaryPath != null)
                    yield return this.NodeBoundaryPath;

                if (this._collapseButtonBorder != null)
                {
                    yield return _collapseButtonBorder;
                    yield return _topMarginRect;
                    yield return _collapseSymbolPath;
                }
            }
        }

        public void UpdateNodeViewerVisuals()
        {
            this.UpdateVisibility(this.Node);

            // recreation of the gemotry graph erases the boundary curve.
            // ist has to be recalculated from the nodes label.

            if (this.Node.IsVisible)
            {
                this.NodeBoundaryPath.UpdateNodeViewerVisual(this.NodeLabel.Width, this.NodeLabel.Height, this.Node);
                this.NodeLabel.UpdateNodeViewerVisual(this.Node);
                this.NodeBoundaryPath.UpdateNodeViewerVisual(this.NodeLabel.Width, this.NodeLabel.Height, this.Node);
                this.NodeBoundaryPath.StrokeThickness = this.PathStrokeThickness;

                Wpf2MsaglConverters.PositionFrameworkElement(this.NodeLabel, this.Node.GeometryNode.Center, 1);
                Panel.SetZIndex(this.NodeBoundaryPath, this.ZIndex);
                Panel.SetZIndex(this.NodeLabel, this.ZIndex + 1);
            }
        }

        #endregion Node Viewer Visuals

        #region Setup Subgraphing

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

        #endregion Setup Subgraphing

        #region Update Subgraphing

        private void UpdateSubGraphVisuals()
        {
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

        #endregion Update Subgraphing

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