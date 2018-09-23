using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        #region Zoom the Graph around a Center point

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

        #endregion Zoom the Graph around a Center point

        #region Dragging the mouse around on the Canvas

        private void PanGraph(MouseEventArgs e)
        {
            if (this.UnderLayout)
                return;

            if (!this.GraphCanvas.IsMouseCaptured)
                this.GraphCanvas.CaptureMouse();

            this.SetTransformFromTwoPoints(e.GetPosition((FrameworkElement)this.GraphCanvas.Parent), this.mouseDownPositionInGraph);

            this.ViewChangeEvent?.Invoke(null, null);
        }

        #endregion Dragging the mouse around on the Canvas
    }
}