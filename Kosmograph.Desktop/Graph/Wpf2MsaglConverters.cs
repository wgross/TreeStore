using DrawingColor = Microsoft.Msagl.Drawing.Color;
using GeometryPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace Kosmograph.Desktop.Graph
{
    public static class Wpf2MsaglConverters
    {
        public static System.Windows.Point ToWpf(this GeometryPoint p) => new System.Windows.Point(p.X, p.Y);

        public static GeometryPoint ToMsagl(this System.Windows.Point p) => new GeometryPoint(p.X, p.Y);

        public static System.Windows.Media.Brush ToWpf(this DrawingColor color) => WpfBrushFromMsaglColor(color.A, color.R, color.G, color.R);

        public static System.Windows.Media.Brush WpfBrushFromMsaglColor(byte colorA, byte colorR, byte colorG, byte colorB) =>
            new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color
            {
                A = colorA,
                R = colorR,
                G = colorG,
                B = colorB
            });

        public static void PositionFrameworkElement(System.Windows.FrameworkElement frameworkElement, GeometryPoint center, double scale)
        {
            PositionFrameworkElement(frameworkElement, center.X, center.Y, scale);
        }

        private static void PositionFrameworkElement(System.Windows.FrameworkElement frameworkElement, double x, double y, double scale)
        {
            if (frameworkElement == null)
                return;
            frameworkElement.RenderTransform = new System.Windows.Media.MatrixTransform(
                new System.Windows.Media.Matrix(scale, 0, 0, -scale, x - scale * frameworkElement.Width / 2, y + scale * frameworkElement.Height / 2));
        }
    }
}