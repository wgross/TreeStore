using Microsoft.Msagl.Drawing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        #region Handle of mouse wheel on Canvas

        //private void GraphCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (e.Delta != 0)
        //    {
        //        const double zoomFractionLocal = 0.9;
        //        var zoomInc = e.Delta < 0 ? zoomFractionLocal : 1.0 / zoomFractionLocal;
        //        this.ZoomAbout(this.ZoomFactor * zoomInc, e.GetPosition(GraphCanvas));
        //        e.Handled = true;
        //    }
        //}

        private void GraphCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = this.Zoom(e.Delta, e.GetPosition(GraphCanvas));
        }

        #endregion Handle of mouse wheel on Canvas

        #region Moving the mouse on the Canvas

        private void GraphCanvasMouseMove(object sender, MouseEventArgs e)
        {
            this.MouseMove?.Invoke(this, CreateMouseEventArgs(e));

            if (e.Handled) return;

            if (Mouse.LeftButton == MouseButtonState.Pressed && (!LayoutEditingEnabled || objectUnderMouseCursor == null))
            {
                if (!this.mouseDownPositionInGraph_initialized)
                {
                    this.mouseDownPositionInGraph = e.GetPosition(GraphCanvas).ToMsagl();
                    this.mouseDownPositionInGraph_initialized = true;
                }

                this.PanGraph(e);
            }
            else
            {
                // Retrieve the coordinate of the mouse position.
                var mouseLocation = e.GetPosition(this.GraphCanvas);
                // Clear the contents of the list used for hit test results.
                this.ObjectUnderMouseCursor = null;
                this.UpdateWithWpfHitObjectUnderMouseOnLocation(mouseLocation, this.MyHitTestResultCallback);
            }
        }

        #endregion Moving the mouse on the Canvas

        #region Pressing/Relaesing the lefft mouse button

        private void GraphCanvasMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.clickCounter.AddMouseDown(objectUnderMouseCursor);
            this.MouseDown?.Invoke(this, this.CreateMouseEventArgs(e));

            if (e.Handled) return;
            mouseDownPositionInGraph = e.GetPosition(GraphCanvas).ToMsagl();
            mouseDownPositionInGraph_initialized = true;
        }

        private void GraphCanvasMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            this.OnMouseUp(e);
            this.clickCounter.AddMouseUp();
            if (this.GraphCanvas.IsMouseCaptured)
            {
                e.Handled = true;
                this.GraphCanvas.ReleaseMouseCapture();
            }
        }

        #endregion Pressing/Relaesing the lefft mouse button

        #region Pressing/Releasing the right mouse button

        private void GraphCanvasRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseDown?.Invoke(this, this.CreateMouseEventArgs(e));
        }

        private void GraphCanvasRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.OnMouseUp(e);
        }

        #endregion Pressing/Releasing the right mouse button

        #region Moving the mouse on the Canvas/Hit test

        private double MouseHitTolerance => (0.05) * DpiX / CurrentScale;

        /// <summary>
        /// Create a <see cref="Rect"/> centered at the given <paramref name="center"/>.
        /// The distance from the center is <paramref name="mouseHitTolerance"/>
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        private Rect MouseHitToleranceRectangle(Point center, double mouseHitTolerance) => new Rect(
                new Point(center.X - mouseHitTolerance, center.Y - mouseHitTolerance),
                new Point(center.X + mouseHitTolerance, center.Y + mouseHitTolerance));

        private void UpdateWithWpfHitObjectUnderMouseOnLocation(Point mousePosition, HitTestResultCallback hitTestResultCallback)
        {
            this._objectUnderMouseDetectionLocation = mousePosition;

            // Expand the hit test area by creating a geometry centered on the hit test point.
            var expandedHitTestArea = new RectangleGeometry(this.MouseHitToleranceRectangle(mousePosition, this.MouseHitTolerance));

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(this.GraphCanvas, null, hitTestResultCallback, new GeometryHitTestParameters(expandedHitTestArea));
        }

        private HitTestResultBehavior MyHitTestResultCallback(HitTestResult result)
        {
            var frameworkElement = result.VisualHit as FrameworkElement;

            if (frameworkElement is null)
                return HitTestResultBehavior.Continue; // no WPF shape under the mouse

            if (frameworkElement.Tag is null)
                return HitTestResultBehavior.Continue; // shape found but is not connected to something in the graph model

            var tag = frameworkElement.Tag;
            var iviewerObj = tag as IViewerObject;
            if (iviewerObj is null)
                return HitTestResultBehavior.Continue; // tag is spneting different; unkown in the graph model.

            if (!iviewerObj.DrawingObject.IsVisible)
                return HitTestResultBehavior.Continue; // graph model item is now visible; continue

            // a visible graph model item was found.
            // always overwrite an edge or take the one with greater zIndex
            if (this.ObjectUnderMouseCursor is IViewerEdge || ObjectUnderMouseCursor is null || (Panel.GetZIndex(frameworkElement) > Panel.GetZIndex(GetFrameworkElementFromIViewerObject(ObjectUnderMouseCursor))))
                this.ObjectUnderMouseCursor = iviewerObj;

            return HitTestResultBehavior.Continue;
        }

        public IViewerObject ObjectUnderMouseCursor
        {
            get
            {
                // this function can bring a stale object
                var location = Mouse.GetPosition(GraphCanvas);
                if (!(_objectUnderMouseDetectionLocation == location))
                    UpdateWithWpfHitObjectUnderMouseOnLocation(location, MyHitTestResultCallbackWithNoCallbacksToTheUser);
                return GetIViewerObjectFromObjectUnderCursor(objectUnderMouseCursor);
            }
            private set
            {
                if (this.objectUnderMouseCursor != value)
                {
                    var oldValue = this.objectUnderMouseCursor;
                    this.objectUnderMouseCursor = value;

                    ObjectUnderMouseCursorChanged?.Invoke(this, new ObjectUnderMouseCursorChangedEventArgs(
                        GetIViewerObjectFromObjectUnderCursor(oldValue), GetIViewerObjectFromObjectUnderCursor(value)));
                }
            }
        }

        private object objectUnderMouseCursor;

        public event EventHandler<ObjectUnderMouseCursorChangedEventArgs> ObjectUnderMouseCursorChanged;

        #endregion Moving the mouse on the Canvas/Hit test
    }
}