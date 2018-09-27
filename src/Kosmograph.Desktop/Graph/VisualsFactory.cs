using Microsoft.Msagl.Core.Geometry.Curves;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawingLabel = Microsoft.Msagl.Drawing.Label;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using DrawingShape = Microsoft.Msagl.Drawing.Shape;
using GeometryRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;

namespace Kosmograph.Desktop.Graph
{
    public static class VisualsFactory
    {
        #region A nodes label is visualized by a TextBlock instance

        public static TextBlock CreateLabel(DrawingLabel drawingLabel)
        {
            return new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
            .UpdateFrom(drawingLabel);
        }

        public static TextBlock UpdateFrom(this TextBlock textBlock, DrawingLabel drawingLabel)
        {
            Debug.Assert(textBlock.Dispatcher.CheckAccess());

            textBlock.Text = drawingLabel.Text ?? string.Empty;
            textBlock.ToolTip = drawingLabel.Text ?? string.Empty;
            textBlock.FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName);
            textBlock.FontSize = drawingLabel.FontSize;
            textBlock.Foreground = drawingLabel.FontColor.ToWpf();

            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;

            return textBlock;
        }

        public static TextBlock UpdateFrom(this TextBlock textBlock, DrawingNode drawingNode)
        {
            Debug.Assert(textBlock.Dispatcher.CheckAccess());

            var tmp = textBlock.UpdateFrom(drawingNode.Label);
            tmp.Width = drawingNode.Width;
            tmp.Height = drawingNode.Height;
            tmp.Margin = new Thickness(drawingNode.Attr.LabelMargin);

            return textBlock;
        }

        #endregion A nodes label is visualized by a TextBlock instance

        #region A node is separated from the background by a boundary Line or Shape.

        public static Path CreateNodeBoundary(DrawingNode drawingNode)
        {
            return new Path().Update(drawingNode);
        }

        public static Path Update(this Path nodeBoundary, DrawingNode drawingNode)
        {
            switch (drawingNode.Attr.Shape)
            {
                case DrawingShape.Box:
                case DrawingShape.House:
                case DrawingShape.InvHouse:
                case DrawingShape.Diamond:
                case DrawingShape.Octagon:
                case DrawingShape.Hexagon:
                    nodeBoundary.Data = ConvertMsaglCurveToPathGeometry(drawingNode.GeometryNode.BoundaryCurve);
                    break;

                case DrawingShape.DoubleCircle:
                    nodeBoundary.Data = ConvertMsaglRectangleToDoubleCirclePathGeometry(drawingNode.BoundingBox);
                    break;

                default:
                    nodeBoundary.Data = ConvertMsaglRectangleToEllipseGeometry(drawingNode.BoundingBox);
                    break;
            }

            nodeBoundary.Stroke = drawingNode.Attr.Color.ToWpf();
            nodeBoundary.Fill = drawingNode.Attr.FillColor.ToWpf();
            return nodeBoundary;
        }

        private static PathGeometry ConvertMsaglRectangleToDoubleCirclePathGeometry(GeometryRectangle box)
        {
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

        private static Geometry ConvertMsaglCurveToPathGeometry(ICurve iCurve)
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
                    // Really?
                    var ellipse = iCurve as Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
                    if (ellipse != null)
                    {
                        return new EllipseGeometry(ellipse.Center.ToWpf(), ellipse.AxisA.Length, ellipse.AxisB.Length);
                    }
                    var poly = iCurve as Microsoft.Msagl.Core.Geometry.Curves.Polyline;
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
                var ls = seg as Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
                if (ls != null)
                    pathFigure.Segments.Add(new System.Windows.Media.LineSegment(ls.End.ToWpf(), true));
                else
                {
                    var ellipse = seg as Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
                    if (ellipse != null)
                        pathFigure.Segments.Add(new ArcSegment(ellipse.End.ToWpf(),
                            new Size(ellipse.AxisA.Length, ellipse.AxisB.Length),
                            Microsoft.Msagl.Core.Geometry.Point.Angle(new Microsoft.Msagl.Core.Geometry.Point(1, 0), ellipse.AxisA),
                            ellipse.ParEnd - ellipse.ParEnd >= Math.PI,
                            !ellipse.OrientedCounterclockwise()
                                ? SweepDirection.Counterclockwise
                                : SweepDirection.Clockwise, true));
                }
            }
        }

        private static EllipseGeometry ConvertMsaglRectangleToEllipseGeometry(GeometryRectangle box)
        {
            return new EllipseGeometry(box.Center.ToWpf(), box.Width / 2, box.Height / 2);
        }

        #endregion A node is separated from the background by a boundary Line or Shape.
    }
}