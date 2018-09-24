using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Miscellaneous;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private System.Windows.Shapes.Rectangle rectToFillCanvas;
        private FrameworkElement _rectToFillGraphBackground;

        public event EventHandler LayoutStarted;

        public event EventHandler LayoutComplete;

        public CancelToken CancelToken { get; set; }

        public bool RunLayoutAsync { get; set; }

        public bool NeedToCalculateLayout
        {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        private GeometryGraph geometryGraphUnderLayout;

        private void LayoutGraph()
        {
            if (this.NeedToCalculateLayout)
            {
                try
                {
                    LayoutHelpers.CalculateLayout(this.geometryGraphUnderLayout, this.Graph.LayoutAlgorithmSettings, this.CancelToken);

                    //if (MsaglFileToSave != null)
                    //{
                    //    drawingGraph.Write(MsaglFileToSave);
                    //    Console.WriteLine("saved into {0}", MsaglFileToSave);
                    //    Environment.Exit(0);
                    //}
                }
                catch (OperationCanceledException)
                {
                    //swallow this exception
                }
            }
        }

        private void PostLayoutStep()
        {
            this.GraphCanvasShow();
            this.PushDataFromLayoutGraphToFrameworkElements();
            this.backgroundWorker = null; //this will signal that we are not under layout anymore
            this.GraphChanged.Invoke(this, null);
            this.SetInitialTransform();
        }

        private void PushDataFromLayoutGraphToFrameworkElements()
        {
            this.DrawGraphBackgound();

            this.CreateVNodes();
            this.CreateEdges();
        }

        #region Draw the background of the graph

        public void DrawGraphBackgound()
        {
            // at the very back (Z order -2) is a transparent background.
            this.rectToFillCanvas = this.CreateRectToFillCanvas();
            this.GraphCanvas.Children.Add(this.rectToFillCanvas);

            this.CreateAndPositionGraphBackgroundRectangle();
        }

        private Rectangle CreateRectToFillCanvas()
        {
            var parent = (Panel)this.GraphCanvas.Parent;

            var rectangle = new System.Windows.Shapes.Rectangle()
            {
                Width = parent.ActualWidth,
                Height = parent.ActualHeight,
                Fill = System.Windows.Media.Brushes.Transparent
            };

            Canvas.SetLeft(rectangle, 0);
            Canvas.SetTop(rectangle, 0);
            Panel.SetZIndex(rectangle, -2);

            return rectangle;
        }

        private void CreateAndPositionGraphBackgroundRectangle()
        {
            this.CreateGraphBackgroundRect();
            this.SetBackgroundRectanglePositionAndSize();

            var rect = this._rectToFillGraphBackground as System.Windows.Shapes.Rectangle;
            if (rect != null)
            {
                rect.Fill = this.drawingGraph.Attr.BackgroundColor.ToWpf();
                //rect.Fill = Brushes.Green;
            }
            Panel.SetZIndex(_rectToFillGraphBackground, -1);
            this.GraphCanvas.Children.Add(this._rectToFillGraphBackground);
        }

        private void CreateGraphBackgroundRect()
        {
            var lgGraphBrowsingSettings = drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings;
            if (lgGraphBrowsingSettings is null)
            {
                this._rectToFillGraphBackground = new System.Windows.Shapes.Rectangle();
            }
        }

        private void SetBackgroundRectanglePositionAndSize()
        {
            if (this.GeometryGraph is null)
                return;
            //            Canvas.SetLeft(_rectToFillGraphBackground, geomGraph.Left);
            //            Canvas.SetTop(_rectToFillGraphBackground, geomGraph.Bottom);
            this._rectToFillGraphBackground.Width = GeometryGraph.Width;
            this._rectToFillGraphBackground.Height = GeometryGraph.Height;

            var center = this.GeometryGraph.BoundingBox.Center;

            Wpf2MsaglConverters.PositionFrameworkElement(_rectToFillGraphBackground, center, 1);
        }

        #endregion Draw the background of the graph
    }
}