using System;
using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private double FitFactor
        {
            get
            {
                var geomGraph = this.GeometryGraph;
                if (drawingGraph == null || geomGraph == null || geomGraph.Width == 0 || geomGraph.Height == 0)
                    return 1;

                return GetFitFactor(this.GraphCanvas.RenderSize);
            }
        }

        private double GetFitFactor(Size rect)
        {
            var geomGraph = GeometryGraph;
            return geomGraph == null ? 1 : Math.Min(rect.Width / geomGraph.Width, rect.Height / geomGraph.Height);
        }

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

        #region Graph size follows Canvas size

        private void GraphCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (drawingGraph == null)
                return;
            // keep the same zoom level
            double oldfit = this.GetFitFactor(e.PreviousSize);
            double fitNow = this.FitFactor;
            double scaleFraction = fitNow / oldfit;

            this.SetTransform(this.CurrentScale * scaleFraction, this.CurrentXOffset * scaleFraction, this.CurrentYOffset * scaleFraction);
        }

        #endregion Graph size follows Canvas size
    }
}