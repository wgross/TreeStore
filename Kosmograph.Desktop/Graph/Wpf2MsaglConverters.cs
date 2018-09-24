namespace Kosmograph.Desktop.Graph
{
    public static class Wpf2MsaglConverters
    {
        public static System.Windows.Point ToWpf(this Microsoft.Msagl.Core.Geometry.Point p) => new System.Windows.Point(p.X, p.Y);

        public static Microsoft.Msagl.Core.Geometry.Point ToMsagl(this System.Windows.Point p) => new Microsoft.Msagl.Core.Geometry.Point(p.X, p.Y);

        public static System.Windows.Media.Brush ToWpf(this Microsoft.Msagl.Drawing.Color color) => WpfBrushFromMsaglColor(color.A, color.R, color.G, color.R);

        public static System.Windows.Media.Brush WpfBrushFromMsaglColor(byte colorA, byte colorR, byte colorG, byte colorB) => new System.Windows.Media.SolidColorBrush(
            new System.Windows.Media.Color
            {
                A = colorA,
                R = colorR,
                G = colorG,
                B = colorB
            });

        public static void PositionFrameworkElement(System.Windows.FrameworkElement frameworkElement, Microsoft.Msagl.Core.Geometry.Point center, double scale)
        {
            PositionFrameworkElement(frameworkElement, center.X, center.Y, scale);
        }

        private static void PositionFrameworkElement(System.Windows.FrameworkElement frameworkElement, double x, double y, double scale)
        {
            if (frameworkElement == null)
                return;
            frameworkElement.RenderTransform =
                new System.Windows.Media.MatrixTransform(new System.Windows.Media.Matrix(scale, 0, 0, -scale, x - scale * frameworkElement.Width / 2,
                    y + scale * frameworkElement.Height / 2));
        }

        public static System.Windows.Controls.TextBlock ToWpf(this Microsoft.Msagl.Drawing.Label drawingLabel)
        {
            var textBlock = new System.Windows.Controls.TextBlock
            {
                Tag = drawingLabel,
                Text = drawingLabel.Text,
                FontFamily = new System.Windows.Media.FontFamily(drawingLabel.FontName),
                FontSize = drawingLabel.FontSize,
                Foreground = drawingLabel.FontColor.ToWpf()
            };

            textBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;
            return textBlock;
        }
    }
}