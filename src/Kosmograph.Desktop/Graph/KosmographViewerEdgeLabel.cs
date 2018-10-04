using Kosmograph.Desktop.Graph.Base;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawingLabel = Microsoft.Msagl.Drawing.Label;

namespace Kosmograph.Desktop.Graph
{
    public class KosmographViewerEdgeLabel : KosmographViewerItemBase, IViewerObject, IInvalidatable
    {
        public DrawingLabel EdgeLabel { get; }

        public KosmographViewerEdgeLabel(DrawingLabel edgeLabel, TextBlock edgeLabelVisual)
        {
            this.EdgeLabel = edgeLabel;
            (this.EdgeLabelVisual, this.AttachmentLine) = this.SetupVisuals(edgeLabelVisual);
        }

        #region IViewerObject members

        public DrawingObject DrawingObject => this.EdgeLabel;

        public bool MarkedForDragging { get; set; }

        public event EventHandler MarkedForDraggingEvent;

        public event EventHandler UnmarkedForDraggingEvent;

        #endregion IViewerObject members

        #region IInvalidate members

        public void Invalidate()
        {
            this.EdgeLabelVisual.InvokeInUiThread(() => this.UpdateVisuals());
        }

        #endregion IInvalidate members

        #region Edge Label Visuals

        public TextBlock EdgeLabelVisual { get; }

        public Line AttachmentLine { get; set; }

        public override IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                yield return this.EdgeLabelVisual;
                yield return this.AttachmentLine;
            }
        }

        private (TextBlock labelVisual, Line attachmentLine) SetupVisuals(TextBlock edgeLabelVisual)
        {
            edgeLabelVisual.Tag = this;  // Tag is used by graph viewers hit test

            this.EdgeLabel.Width = edgeLabelVisual.DesiredSize.Width;
            this.EdgeLabel.Height = edgeLabelVisual.DesiredSize.Height;
            var attachmentLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeDashArray = new DoubleCollection(new double[] { 1, 2 }),
                Visibility = System.Windows.Visibility.Hidden
            };
            return (edgeLabelVisual, attachmentLine);
        }

        private void UpdateVisuals()
        {
            this.EdgeLabelVisual.UpdateFrom(this.EdgeLabel, measure: true);

            Wpf2MsaglConverters.PositionFrameworkElement(this.EdgeLabelVisual, this.EdgeLabel.Center, 1);

            if (this.MarkedForDragging)
            {
                this.AttachmentLine.Visibility = System.Windows.Visibility.Visible;

                var geomLabel = this.EdgeLabel.GeometryLabel;
                this.AttachmentLine.X1 = geomLabel.AttachmentSegmentStart.X;
                this.AttachmentLine.Y1 = geomLabel.AttachmentSegmentStart.Y;
                this.AttachmentLine.X2 = geomLabel.AttachmentSegmentEnd.X;
                this.AttachmentLine.Y2 = geomLabel.AttachmentSegmentEnd.Y;
            }
            else this.AttachmentLine.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion Edge Label Visuals
    }
}