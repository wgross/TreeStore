using Microsoft.Msagl.Core.Geometry.Curves;
using System;
using System.Windows;
using System.Windows.Media;

using DrawingNode = Microsoft.Msagl.Drawing.Node;
using DrawingShape = Microsoft.Msagl.Drawing.Shape;

namespace Kosmograph.Desktop.Graph
{
    /// <summary>
    /// Depening on the Shape or Bounding Boxc of the DrawingNode a conversion to WPF class is made.
    /// </summary>
    public class NodeBoundaryGeometry
    {
        public static Geometry Create(DrawingNode drawingNode)
        {
            switch (drawingNode.Attr.Shape)
            {
                case DrawingShape.Box:
                case DrawingShape.House:
                case DrawingShape.InvHouse:
                case DrawingShape.Diamond:
                case DrawingShape.Octagon:
                case DrawingShape.Hexagon:
                    return NodeBoundaryGeometry.ConvertMsaglCurveToPathGeometry(drawingNode.GeometryNode.BoundaryCurve);

                case DrawingShape.DoubleCircle:
                    return NodeBoundaryGeometry.ConvertMsaglRectangleToDoubleCirclePathGeometry(drawingNode.BoundingBox);

                default:
                    return NodeBoundaryGeometry.ConvertMsaglRectangleToEllipseGeometry(drawingNode.BoundingBox);
            }
        }

        private static PathGeometry ConvertMsaglRectangleToDoubleCirclePathGeometry(Microsoft.Msagl.Core.Geometry.Rectangle box)
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
                var ls = seg as Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
                if (ls != null)
                    pathFigure.Segments.Add(new System.Windows.Media.LineSegment(ls.End.ToWpf(), true));
                else
                {
                    var ellipse = seg as Ellipse;
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

        private static EllipseGeometry ConvertMsaglRectangleToEllipseGeometry(Microsoft.Msagl.Core.Geometry.Rectangle box)
        {
            return new EllipseGeometry(box.Center.ToWpf(), box.Width / 2, box.Height / 2);
        }
    }
}