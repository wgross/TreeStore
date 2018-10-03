using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using DrawingLabel = Microsoft.Msagl.Drawing.Label;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerEdgeLabel : IViewerObject, IInvalidatable
    {
        public KosmographViewerEdgeLabel(DrawingLabel edgeLabel, TextBlock edgeLabelVisual)
        {
            this.EdgeLabel = edgeLabel;
            this.EdgeLabelVisual = this.SetupVisuals(edgeLabelVisual);
        }

        #region IViewerObject members

        public DrawingObject DrawingObject => this.EdgeLabel;

        public bool MarkedForDragging
        {
            get => this.markedForDragging;
            set
            {
                this.markedForDragging = value;
                if (value)
                {
                    this.AttachmentLine = new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeDashArray = new System.Windows.Media.DoubleCollection(OffsetElems())
                    }; //the line will have 0,0, 0,0 start and end so it would not be rendered

                    ((Canvas)EdgeLabelVisual.Parent).Children.Add(AttachmentLine);
                }
                else
                {
                    ((Canvas)EdgeLabelVisual.Parent).Children.Remove(AttachmentLine);
                    this.AttachmentLine = null;
                }
            }
        }

        private bool markedForDragging;

        public event EventHandler MarkedForDraggingEvent;

        public event EventHandler UnmarkedForDraggingEvent;

        #endregion IViewerObject members

        private IEnumerable<double> OffsetElems()
        {
            yield return 1;
            yield return 2;
        }

        #region IInvalidate members

        public void Invalidate()
        {
            this.EdgeLabelVisual.InvokeInUiThread(() => this.UpdateVisuals());
        }

        #endregion IInvalidate members

        #region Edge Label Visuals

        public TextBlock EdgeLabelVisual { get; }

        public DrawingLabel EdgeLabel { get; }

        private Line AttachmentLine { get; set; }

        private TextBlock SetupVisuals(TextBlock edgeLabelVisual)
        {
            this.EdgeLabel.Width = edgeLabelVisual.DesiredSize.Width;
            this.EdgeLabel.Height = edgeLabelVisual.DesiredSize.Height;
            return edgeLabelVisual;
        }

        private void UpdateVisuals()
        {
            this.EdgeLabelVisual.UpdateFrom(this.EdgeLabel, measure: true);

            Wpf2MsaglConverters.PositionFrameworkElement(this.EdgeLabelVisual, this.EdgeLabel.Center, 1);

            var geomLabel = this.EdgeLabel.GeometryLabel;

            if (this.AttachmentLine is null)
                return;

            this.AttachmentLine.X1 = geomLabel.AttachmentSegmentStart.X;
            this.AttachmentLine.Y1 = geomLabel.AttachmentSegmentStart.Y;
            this.AttachmentLine.X2 = geomLabel.AttachmentSegmentEnd.X;
            this.AttachmentLine.Y2 = geomLabel.AttachmentSegmentEnd.Y;
        }

        #endregion Edge Label Visuals
    }
}