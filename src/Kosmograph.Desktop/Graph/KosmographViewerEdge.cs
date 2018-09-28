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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerEdge : KosmographViewerItemBase, IViewerEdge, IInvalidatable
    {
        public KosmographViewerEdge(DrawingEdge edge, FrameworkElement labelFrameworkElement)
        {
            this.Edge = edge;
            this.EdgePath = new Path
            {
                Data = VisualsFactory.CreateEdgePath(this.Edge.GeometryEdge.Curve),
                Tag = this
            };

            (this.EdgeLabel, this.EdgePath, this.SourceArrowHeadPath, this.TargetArrowHeadPath) = this.SetupEdgeVisuals(labelFrameworkElement);
            this.SetPathStroke();

            Wpf2MsaglConverters.PositionFrameworkElement(this.EdgeLabel, this.Edge.Label.Center, 1);

            this.Edge.Attr.VisualsChanged += (a, b) => Invalidate();
            this.Edge.IsVisibleChanged += this.UpdateVisibility;
        }

        public KosmographViewerEdge(DrawingEdge edge, LgLayoutSettings lgSettings)
        {
            Edge = edge;
        }

        #region Edge viewer is composed of multiple visual elements

        public FrameworkElement EdgeLabel;

        public Path EdgePath { get; }

        public Path SourceArrowHeadPath { get; }

        public Path TargetArrowHeadPath { get; }

        override public IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                if (this.SourceArrowHeadPath != null)
                    yield return this.SourceArrowHeadPath;
                if (this.TargetArrowHeadPath != null)
                    yield return this.TargetArrowHeadPath;

                if (this.EdgePath != null)
                    yield return this.EdgePath;

                if (this.EdgeLabel != null)
                    yield return this.EdgeLabel;
            }
        }

        #endregion Edge viewer is composed of multiple visual elements

        #region Setup Edge viewers visuals

        public (FrameworkElement edgeLabel, Path edgePath, Path edgeSourceArrow, Path edgeTargetArrow) SetupEdgeVisuals(FrameworkElement edgeLabel)
        {
            // nope: this must be VLabel//edgeLabel.Tag = this;

            var edgePath = new Path
            {
                Data = VisualsFactory.CreateEdgePath(this.Edge.GeometryEdge.Curve),
                Tag = this
            };

            var edgeSourceArrow = this.Edge.Attr.ArrowAtSource ?
                new Path
                {
                    Data = VisualsFactory.CreateEdgeSourceArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness),
                    Tag = this
                } : null;

            var edgeTargetArrow = this.Edge.Attr.ArrowAtTarget ?
                new Path
                {
                    Data = VisualsFactory.CreateEdgeTargetArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness),
                    Tag = this
                } : null;

            return (edgeLabel, edgePath, edgeSourceArrow, edgeTargetArrow);
        }

        #endregion Setup Edge viewers visuals

        private double PathStrokeThickness
        {
            get
            {
                return PathStrokeThicknessFunc != null ? PathStrokeThicknessFunc() : this.Edge.Attr.LineWidth;
            }
        }

        #region IViewerObject Members

        public DrawingObject DrawingObject => this.Edge;

        public bool MarkedForDragging { get; set; }

        public event EventHandler MarkedForDraggingEvent;

        public event EventHandler UnmarkedForDraggingEvent;

        #endregion IViewerObject Members

        #region IViewerEdge Members

        public DrawingEdge Edge { get; private set; }

        public IViewerNode Source { get; private set; }

        public IViewerNode Target { get; private set; }

        public double RadiusOfPolylineCorner { get; set; }

        public VLabel VLabel { get; set; }

        #endregion IViewerEdge Members

        #region IInvalidate members

        public void Invalidate(FrameworkElement fe, Rail rail, byte edgeTransparency)
        {
            var path = fe as Path;
            if (path != null)
                SetPathStrokeToRailPath(rail, path, edgeTransparency);
        }

        public void Invalidate()
        {
            this.UpdateVisibility(this.Edge);
            if (!this.Edge.IsVisible)
                return;

            this.EdgePath.Data = VisualsFactory.CreateEdgePath(Edge.GeometryEdge.Curve);

            // arrows should be nulled and removed if they are nor required anymore.
            // revist on graphic property editing
            // --wgross 28.09.2018
            if (this.Edge.Attr.ArrowAtSource)
                SourceArrowHeadPath.Data = VisualsFactory.CreateEdgeSourceArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness);
            if (this.Edge.Attr.ArrowAtTarget)
                TargetArrowHeadPath.Data = VisualsFactory.CreateEdgeTargetArrow(this.Edge.GeometryEdge.EdgeGeometry, this.PathStrokeThickness);

            this.SetPathStroke();
            if (VLabel != null)
                ((IInvalidatable)VLabel).Invalidate();
        }

        #endregion IInvalidate members

        private void SetPathStroke()
        {
            SetPathStrokeToPath(EdgePath);
            if (SourceArrowHeadPath != null)
            {
                SourceArrowHeadPath.Stroke = SourceArrowHeadPath.Fill = Edge.Attr.Color.ToWpf();
                SourceArrowHeadPath.StrokeThickness = PathStrokeThickness;
            }
            if (TargetArrowHeadPath != null)
            {
                TargetArrowHeadPath.Stroke = TargetArrowHeadPath.Fill = Edge.Attr.Color.ToWpf();
                TargetArrowHeadPath.StrokeThickness = PathStrokeThickness;
            }
        }

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

        private void SetPathStrokeToPath(Path path)
        {
            path.Stroke = Edge.Attr.Color.ToWpf();
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

        public override string ToString()
        {
            return Edge.ToString();
        }

        public static double _dashSize = 0.05; //inches
        public Func<double> PathStrokeThicknessFunc;

        public double DashSize()
        {
            var w = PathStrokeThickness;
            var dashSizeInPoints = _dashSize * KosmographViewer.DpiXStatic;
            return dashSizeInPoints / w;
        }

        public void RemoveItselfFromCanvas(Canvas graphCanvas)
        {
            if (EdgePath != null)
                graphCanvas.Children.Remove(EdgePath);

            if (SourceArrowHeadPath != null)
                graphCanvas.Children.Remove(SourceArrowHeadPath);

            if (TargetArrowHeadPath != null)
                graphCanvas.Children.Remove(TargetArrowHeadPath);

            if (VLabel != null)
                graphCanvas.Children.Remove(VLabel.FrameworkElement);
        }
    }
}