using System.Windows;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        #region Zoom the Graph

        public double ZoomFactor => this.CurrentScale / this.FitFactor;

        private Point ZoomCenterPoint(Point centerOfZoom) => this.GraphCanvas
                .TransformToAncestor((FrameworkElement)this.GraphCanvas.Parent)
                .Transform(centerOfZoom);

        public bool Zoom(int zoomSteps, Point centerOfZoom)
        {
            if (zoomSteps == 0)
                return false;

            const double zoomFractionLocal = 0.9;
            var zoomInc = zoomSteps < 0 ? zoomFractionLocal : 1.0 / zoomFractionLocal;
            this.Zoom(this.ZoomFactor * zoomInc, centerOfZoom);

            return true;
        }

        public void Zoom(double zoomFactor, Point centerOfZoom)
        {
            var scale = zoomFactor * this.FitFactor;
            var centerOfZoomOnScreen = this.ZoomCenterPoint(centerOfZoom);

            this.SetTransform(scale, centerOfZoomOnScreen.X - centerOfZoom.X * scale, centerOfZoomOnScreen.Y + centerOfZoom.Y * scale);
        }

        #endregion Zoom the Graph
    }
}