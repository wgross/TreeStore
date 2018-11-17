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

using Kosmograph.Desktop.Graph.Base;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerEdge : KosmographViewerItemBase, IViewerEdge, IInvalidatable
    {
        #region Construction and initialization of this instance

        public KosmographViewerEdge(DrawingEdge edge, KosmographViewerEdgeLabel edgeLabelViewer)
        {
            this.Edge = edge;
            this.EdgeLabelViewer = edgeLabelViewer;

            (this.EdgePath, this.SourceArrowHeadPath, this.TargetArrowHeadPath) = this.SetupEdgeVisuals();

            this.Edge.Attr.VisualsChanged += (a, b) => Invalidate();
            this.Edge.IsVisibleChanged += this.UpdateVisibility;
        }

        public KosmographViewerEdge(DrawingEdge edge, LgLayoutSettings lgSettings)
        {
            Edge = edge;
        }

        #endregion Construction and initialization of this instance

        #region An edge shows an additional edge label viewer

        public KosmographViewerEdgeLabel EdgeLabelViewer { get; }

        public TextBlock EdgeLabel => this.EdgeLabelViewer?.EdgeLabelVisual;

        #endregion An edge shows an additional edge label viewer

        #region An edge is displayed using multiple visual elements

        public Path EdgePath { get; }

        public Path SourceArrowHeadPath { get; }

        public Path TargetArrowHeadPath { get; }

        override public IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                return new FrameworkElement[]
                {
                    this.SourceArrowHeadPath,
                    this.TargetArrowHeadPath,
                    this.EdgePath,
                }
                .Concat(this.EdgeLabelViewer.FrameworkElements);
            }
        }

        public (Path edgePath, Path edgeSourceArrow, Path edgeTargetArrow) SetupEdgeVisuals()
        {
            Path emptyPath(Visibility visibility) => new Path { Tag = this, Visibility = visibility };

            return (emptyPath(Visibility.Visible), emptyPath(Visibility.Hidden), emptyPath(Visibility.Hidden));
        }

        /// <summary>
        /// The properties of this edges visuals are updated from the current state of the dunerlying <see cref="DrawingEdge"/>
        /// </summary>
        public void UpdateEdgeVisuals()
        {
            this.EdgePath.Data = VisualsFactory.CreateEdgePath(this.Edge.GeometryEdge.Curve);
            this.EdgePath.Stroke = this.Edge.Attr.Color.ToWpf();
            this.EdgePath.StrokeThickness = this.PathStrokeThickness;

            foreach (var style in this.Edge.Attr.Styles)
            {
                if (style == Microsoft.Msagl.Drawing.Style.Dotted)
                {
                    this.EdgePath.StrokeDashArray = new DoubleCollection { 1, 1 };
                }
                else if (style == Microsoft.Msagl.Drawing.Style.Dashed)
                {
                    var f = this.DashSize();
                    this.EdgePath.StrokeDashArray = new DoubleCollection { f, f };
                    //CurvePath.StrokeDashOffset = f;
                }
            }

            if (this.Edge.Attr.ArrowAtSource)
            {
                this.SourceArrowHeadPath.Data = VisualsFactory.CreateEdgeSourceArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness);
                this.SourceArrowHeadPath.Stroke = this.SourceArrowHeadPath.Fill = Edge.Attr.Color.ToWpf();
                this.SourceArrowHeadPath.StrokeThickness = this.PathStrokeThickness;
                this.SourceArrowHeadPath.Visibility = Visibility.Visible;
            }
            else this.SourceArrowHeadPath.Visibility = Visibility.Hidden;

            if (this.Edge.Attr.ArrowAtTarget)
            {
                this.TargetArrowHeadPath.Data = VisualsFactory.CreateEdgeTargetArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness);
                this.TargetArrowHeadPath.Stroke = this.TargetArrowHeadPath.Fill = this.Edge.Attr.Color.ToWpf();
                this.TargetArrowHeadPath.StrokeThickness = this.PathStrokeThickness;
                this.TargetArrowHeadPath.Visibility = Visibility.Visible;
            }
            else this.TargetArrowHeadPath.Visibility = Visibility.Hidden;
        }

        #endregion An edge is displayed using multiple visual elements

        private double PathStrokeThickness => this.PathStrokeThicknessFunc is null ? this.Edge.Attr.LineWidth : this.PathStrokeThicknessFunc();

        public Func<double> PathStrokeThicknessFunc { private get; set; }

        #region IViewerObject Members

        public DrawingObject DrawingObject => this.Edge;

        #endregion IViewerObject Members

        #region IViewerEdge Members

        public DrawingEdge Edge { get; private set; }

        public IViewerNode Source { get; private set; }

        public IViewerNode Target { get; private set; }

        public double RadiusOfPolylineCorner { get; set; }

        #endregion IViewerEdge Members

        #region IInvalidate Members

        public void Invalidate(FrameworkElement fe, Rail rail, byte edgeTransparency)
        {
            var path = fe as Path;
            if (path != null)
                SetPathStrokeToRailPath(rail, path, edgeTransparency);
        }

        public void Invalidate()
        {
            this.EdgeLabel.InvokeInUiThread(() =>
            {
                this.UpdateVisibility(this.Edge);
                if (!this.Edge.IsVisible)
                    return;

                this.UpdateEdgeVisuals();
                this.EdgeLabelViewer.Invalidate();
            });
        }

        #endregion IInvalidate Members

        private void SetPathStrokeToRailPath(Rail rail, Path path, byte transparency)
        {
            path.Stroke = SetStrokeColorForRail(transparency, rail);
            path.StrokeThickness = PathStrokeThickness;

            foreach (var style in Edge.Attr.Styles)
            {
                if (style == Microsoft.Msagl.Drawing.Style.Dotted)
                {
                    path.StrokeDashArray = new DoubleCollection { 1, 1 };
                }
                else if (style == Microsoft.Msagl.Drawing.Style.Dashed)
                {
                    var f = DashSize();
                    path.StrokeDashArray = new DoubleCollection { f, f };
                    //CurvePath.StrokeDashOffset = f;
                }
            }
        }

        private Brush SetStrokeColorForRail(byte transparency, Rail rail)
        {
            return rail.IsHighlighted == false
                       ? new SolidColorBrush(new System.Windows.Media.Color
                       {
                           A = transparency,
                           R = Edge.Attr.Color.R,
                           G = Edge.Attr.Color.G,
                           B = Edge.Attr.Color.B
                       })
                       : Brushes.Red;
        }

        public override string ToString()
        {
            return Edge.ToString();
        }

        #region Length of Dashes releative to the viewer DPI

        private static double EdgeDashSize = 0.05; //inches

        private double DashSize() => (KosmographViewerEdge.EdgeDashSize * KosmographViewer.DpiXStatic) / this.PathStrokeThickness;

        #endregion Length of Dashes releative to the viewer DPI

        public void RemoveItselfFromCanvas(Canvas graphCanvas)
        {
            if (EdgePath != null)
                graphCanvas.Children.Remove(EdgePath);

            if (SourceArrowHeadPath != null)
                graphCanvas.Children.Remove(SourceArrowHeadPath);

            if (TargetArrowHeadPath != null)
                graphCanvas.Children.Remove(TargetArrowHeadPath);

            if (EdgeLabelViewer != null)
                graphCanvas.Children.Remove(EdgeLabelViewer.EdgeLabelVisual);
        }
    }
}