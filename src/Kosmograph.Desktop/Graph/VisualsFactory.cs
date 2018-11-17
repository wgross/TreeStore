using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawingLabel = Microsoft.Msagl.Drawing.Label;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using DrawingShape = Microsoft.Msagl.Drawing.Shape;
using GeometryEllipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using GeometryLineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using GeometryPoint = Microsoft.Msagl.Core.Geometry.Point;
using GeometryPolyline = Microsoft.Msagl.Core.Geometry.Curves.Polyline;
using GeometryRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;

namespace Kosmograph.Desktop.Graph
{
    public static class VisualsFactory
    {
        #region Microsoft.Msagl.Drawing.Label -> System.Windows.Controls.TextBlock (edge/node label)

        /// <summary>
        /// Create a <see cref="TextBlock"/> from a MSAGL <see cref="Microsoft.Msagl.Drawing.Label"/>.
        /// The box is configured from the node attributes and measured to an ideal size.
        /// </summary>
        /// <param name="drawingLabel"></param>
        /// <returns></returns>
        public static TextBlock CreateLabel(DrawingLabel drawingLabel)
        {
            return new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
            .UpdateFrom(drawingLabel, measure: true);
        }

        /// <summary>
        /// Updates a <see cref="TextBlock"/> from a MSAGL <see cref="Microsoft.Msagl.Drawing.Label"/>.
        /// Width an height of the box are iverwritten be the nodes Width and Height.
        /// </summary>
        /// <returns></returns>
        public static TextBlock UpdateNodeViewerVisual(this TextBlock textBlock, DrawingNode drawingNode)
        {
            Debug.Assert(textBlock.Dispatcher.CheckAccess());

            // margisn could be included in the calcuilatin of the boundary curve instead.
            // maybe this very place of code would be cleaner then.
            textBlock.Margin = new Thickness(drawingNode.Attr.LabelMargin);
            textBlock.UpdateFrom(drawingNode.Label, measure: false);

            return textBlock;
        }

        /// <summary>
        /// Updates a <see cref="TextBlock"/> from a MSAGL <see cref="Microsoft.Msagl.Drawing.Label"/>.
        /// the box will be remeasured.
        /// </summary>
        /// <returns></returns>
        public static TextBlock UpdateFrom(this TextBlock textBlock, DrawingLabel drawingLabel, bool measure)
        {
            Debug.Assert(textBlock.Dispatcher.CheckAccess());

            textBlock.Text = drawingLabel.Text ?? string.Empty;
            textBlock.ToolTip = drawingLabel.Text ?? string.Empty;
            textBlock.FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName);
            textBlock.FontSize = drawingLabel.FontSize;
            textBlock.Foreground = drawingLabel.FontColor.ToWpf();

            if (measure)
            {
                textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                textBlock.Width = textBlock.DesiredSize.Width;
                textBlock.Height = textBlock.DesiredSize.Height;
            }

            return textBlock;
        }

        #endregion Microsoft.Msagl.Drawing.Label -> System.Windows.Controls.TextBlock (edge/node label)

        #region Microsoft.Msagl.Drawing.Node -> System.Windows.Shapes.Path (node boundary)

        public static Path UpdateNodeViewerVisual(this Path nodeBoundary, double nodeLabelWidth, double nodeLabelHeight, DrawingNode drawingNode)
        {
            if (drawingNode.GeometryNode.BoundaryCurve is null)
            {
                // the boundary curve is recalculated from the size of the
                // nodes label visual.

                var width = nodeLabelWidth + 2 * drawingNode.Attr.LabelMargin;
                var height = nodeLabelHeight + 2 * drawingNode.Attr.LabelMargin;
                drawingNode.GeometryNode.BoundaryCurve = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
            }

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

            nodeBoundary.StrokeThickness = drawingNode.Attr.LineWidth;
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

        #endregion Microsoft.Msagl.Drawing.Node -> System.Windows.Shapes.Path (node boundary)

        #region Microsoft.Msagl.Core.Geometry.Curves.ICurve -> System.Windows.Media.Geometry (edge path)

        static public Geometry CreateEdgePath(ICurve curve)
        {
            var streamGeometry = new StreamGeometry();
            if (curve is null)
                return streamGeometry;

            using (StreamGeometryContext context = streamGeometry.Open())
                CreateEdgePathFromCurve(context, curve);
            return streamGeometry;
        }

        static private void CreateEdgePathFromCurve(StreamGeometryContext context, ICurve iCurve)
        {
            context.BeginFigure(iCurve.Start.ToWpf(), false, false);

            var c = iCurve as Curve;
            if (c != null)
            {
                FillContexForCurve(context, c);
                return;
            }

            var cubicBezierSeg = iCurve as CubicBezierSegment;
            if (cubicBezierSeg != null)
            {
                context.BezierTo(cubicBezierSeg.B(1).ToWpf(), cubicBezierSeg.B(2).ToWpf(), cubicBezierSeg.B(3).ToWpf(), true, false);
                return;
            }

            var ls = iCurve as GeometryLineSegment;
            if (ls != null)
            {
                context.LineTo(ls.End.ToWpf(), true, false);
                return;
            }

            var rr = iCurve as RoundedRect;
            if (rr != null)
            {
                FillContexForCurve(context, rr.Curve);
                return;
            }

            var poly = iCurve as GeometryPolyline;
            if (poly != null)
            {
                FillContexForPolyline(context, poly);
                return;
            }

            var ellipse = iCurve as GeometryEllipse;
            if (ellipse != null)
            {
                //       context.LineTo(Common.WpfPoint(ellipse.End),true,false);
                double sweepAngle = EllipseSweepAngle(ellipse);
                bool largeArc = Math.Abs(sweepAngle) >= Math.PI;
                GeometryRectangle box = ellipse.FullBox();
                context.ArcTo(ellipse.End.ToWpf(),
                                new Size(box.Width / 2, box.Height / 2),
                                sweepAngle,
                                largeArc,
                                sweepAngle < 0
                                    ? SweepDirection.Counterclockwise
                                    : SweepDirection.Clockwise,
                                true, true);

                return;
            }
            else throw new NotImplementedException($"{iCurve.GetType()}");
        }

        private static void FillContexForPolyline(StreamGeometryContext context, GeometryPolyline poly)
        {
            for (PolylinePoint pp = poly.StartPoint.Next; pp != null; pp = pp.Next)
                context.LineTo(pp.Point.ToWpf(), true, false);
        }

        private static void FillContexForCurve(StreamGeometryContext context, Curve c)
        {
            foreach (ICurve seg in c.Segments)
            {
                var bezSeg = seg as CubicBezierSegment;
                if (bezSeg != null)
                {
                    context.BezierTo(bezSeg.B(1).ToWpf(), bezSeg.B(2).ToWpf(), bezSeg.B(3).ToWpf(), true, false);
                }
                else
                {
                    var ls = seg as GeometryLineSegment;
                    if (ls != null)
                        context.LineTo(ls.End.ToWpf(), true, false);
                    else
                    {
                        var ellipse = seg as GeometryEllipse;
                        if (ellipse != null)
                        {
                            //       context.LineTo(Common.WpfPoint(ellipse.End),true,false);
                            double sweepAngle = EllipseSweepAngle(ellipse);
                            bool largeArc = Math.Abs(sweepAngle) >= Math.PI;
                            GeometryRectangle box = ellipse.FullBox();
                            context.ArcTo(ellipse.End.ToWpf(),
                                          new Size(box.Width / 2, box.Height / 2),
                                          sweepAngle,
                                          largeArc,
                                          sweepAngle < 0
                                              ? SweepDirection.Counterclockwise
                                              : SweepDirection.Clockwise,
                                          true, true);
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }
        }

        public static double EllipseSweepAngle(GeometryEllipse ellipse)
        {
            double sweepAngle = ellipse.ParEnd - ellipse.ParStart;
            return ellipse.OrientedCounterclockwise() ? sweepAngle : -sweepAngle;
        }

        #endregion Microsoft.Msagl.Core.Geometry.Curves.ICurve -> System.Windows.Media.Geometry (edge path)

        #region Microsoft.Msagl.Core.Layout.EdgeGeometry -> System.Windows.Media.Geometry (edge arrows)

        public static Geometry CreateEdgeSourceArrow(EdgeGeometry edgeGeometry, double pathStrokeThickness)
        {
            var streamGeometry = new StreamGeometry();
            using (StreamGeometryContext context = streamGeometry.Open())
                CreateEdgeArrow(context, edgeGeometry.Curve.Start, edgeGeometry.SourceArrowhead.TipPosition, pathStrokeThickness);

            return streamGeometry;
        }

        public static Geometry CreateEdgeTargetArrow(EdgeGeometry edgeGeometry, double pathStrokeThickness)
        {
            var streamGeometry = new StreamGeometry();
            using (StreamGeometryContext context = streamGeometry.Open())
                CreateEdgeArrow(context, edgeGeometry.Curve.End, edgeGeometry.TargetArrowhead.TipPosition, pathStrokeThickness);

            return streamGeometry;
        }

        private static void CreateEdgeArrow(StreamGeometryContext context, GeometryPoint start, GeometryPoint end, double pathStrokeThickness)
        {
            if (pathStrokeThickness > 1)
                CreateWideEdgeArrow(context, start, end, pathStrokeThickness);
            else
                CreateThinEdgeArrow(context, start, end, pathStrokeThickness);
        }

        private static readonly double HalfArrowAngleTan = Math.Tan(ArrowAngle * 0.5 * Math.PI / 180.0);
        private static readonly double HalfArrowAngleCos = Math.Cos(ArrowAngle * 0.5 * Math.PI / 180.0);
        private const double ArrowAngle = 30.0; //degrees

        private static void CreateWideEdgeArrow(StreamGeometryContext context, GeometryPoint start, GeometryPoint end, double pathStrokeThickness)

        {
            GeometryPoint dir = end - start;
            GeometryPoint h = dir;
            double dl = dir.Length;
            if (dl < 0.001)
                return;
            dir /= dl;

            var s = new GeometryPoint(-dir.Y, dir.X);
            double w = 0.5 * pathStrokeThickness;
            GeometryPoint s0 = w * s;

            s *= h.Length * HalfArrowAngleTan;
            s += s0;

            double rad = w / HalfArrowAngleCos;

            context.BeginFigure((start + s).ToWpf(), true, true);
            context.LineTo((start - s).ToWpf(), true, false);
            context.LineTo((end - s0).ToWpf(), true, false);
            context.ArcTo((end + s0).ToWpf(), new Size(rad, rad), Math.PI - ArrowAngle, false, SweepDirection.Clockwise, true, false);
        }

        private static void CreateThinEdgeArrow(StreamGeometryContext context, GeometryPoint start, GeometryPoint end, double pathStrokeThickness)
        {
            GeometryPoint dir = end - start;
            double dl = dir.Length;
            //take into account the widths
            double delta = Math.Min(dl / 2, pathStrokeThickness + pathStrokeThickness / 2);
            dir *= (dl - delta) / dl;
            end = start + dir;
            dir = dir.Rotate(Math.PI / 2);
            GeometryPoint s = dir * HalfArrowAngleTan;

            context.BeginFigure((start + s).ToWpf(), true, true);
            context.LineTo(end.ToWpf(), true, true);
            context.LineTo((start - s).ToWpf(), true, true);
        }

        #endregion Microsoft.Msagl.Core.Layout.EdgeGeometry -> System.Windows.Media.Geometry (edge arrows)

        #region Microsoft.Msagl.Core.Layout.Rail -> System.Windows.Shapes.Path (edge rail)

        public static Path CreateEdgeRail(Rail rail, byte edgeTransparency, double pathStrokeThickness)
        {
            var railGeometryCurve = rail.Geometry as ICurve;

            Path edgeRailPath;
            if (railGeometryCurve is null)
            {
                var arrowhead = rail.Geometry as Arrowhead;
                if (arrowhead != null)
                {
                    edgeRailPath = CreateEdgeRailArrowhead(rail, arrowhead, rail.CurveAttachmentPoint, edgeTransparency, pathStrokeThickness);
                }
                else throw new InvalidOperationException();
            }
            else
            {
                edgeRailPath = CreateFrameworkElementForRailCurve(rail, railGeometryCurve, edgeTransparency);
            }
            edgeRailPath.Tag = rail;
            return edgeRailPath;
        }

        private static Path CreateEdgeRailArrowhead(Rail rail, Arrowhead arrowhead, GeometryPoint curveAttachmentPoint, byte edgeTransparency, double pathStrokeThickness)
        {
            var streamGeometry = new StreamGeometry();

            using (StreamGeometryContext context = streamGeometry.Open())
                CreateEdgeArrow(context, curveAttachmentPoint, arrowhead.TipPosition, pathStrokeThickness);

            var path = new Path
            {
                Data = streamGeometry,
            };

            return path;
        }

        private static Path CreateFrameworkElementForRailCurve(Rail rail, ICurve iCurve, byte transparency)
        {
            var path = new Path
            {
                Data = VisualsFactory.CreateEdgePath(iCurve),
            };

            return path;
        }

        #endregion Microsoft.Msagl.Core.Layout.Rail -> System.Windows.Shapes.Path (edge rail)
    }
}