using System.Windows.Input;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographControl
    {
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.msaglGraphViewer.OnMouseDragged(e);
            }
            else
            {
                this.msaglGraphViewer.OnMouseMoved(e);
            }
            base.OnMouseMove(e);
        }

        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    this.clickCounter.AddMouseDown(_objectUnderMouseCursor);
        //    this.MouseDown?.Invoke(this, this.CreateMouseEventArgs(e));

        //    if (e.Handled) return;
        //    mouseDownPositionInGraph = e.GetPosition(GraphCanvas).ToMsagl();
        //    mouseDownPositionInGraph_initialized = true;
        //}
    }
}