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
        #region Moving the mouse on the Canvas triggers the hit test and sets the 'object under cursor'

        public void OnMouseMoved(MouseEventArgs e)
        {
            // Clear the contents of the list used for hit test results.
            this.ObjectUnderMouseCursor = null;
            this.UpdateWithWpfHitObjectUnderMouseOnLocation(e.GetPosition(this.GraphCanvas), MyHitTestResultCallback);
        }

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
            _objectUnderMouseDetectionLocation = mousePosition;

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

        #endregion Moving the mouse on the Canvas triggers the hit test and sets the 'object under cursor'

        #region Dragging the mouse on the Canvas starts the Panning

        internal void OnMouseDragged(MouseEventArgs e)
        {
            if (!LayoutEditingEnabled || this.objectUnderMouseCursor is null)
            {
                if (!this.mouseDownPositionInGraph_initialized)
                {
                    this.mouseDownPositionInGraph = e.GetPosition(GraphCanvas).ToMsagl();
                    this.mouseDownPositionInGraph_initialized = true;
                }

                this.Pan(e);
            }
        }

        private void Pan(MouseEventArgs e)
        {
            if (this.UnderLayout)
                return;

            if (!this.GraphCanvas.IsMouseCaptured)
                this.GraphCanvas.CaptureMouse();

            SetTransformFromTwoPoints(e.GetPosition((FrameworkElement)this.GraphCanvas.Parent), this.mouseDownPositionInGraph);

            this.ViewChangeEvent?.Invoke(null, null);
        }

        #endregion Dragging the mouse on the Canvas starts the Panning
    }
}