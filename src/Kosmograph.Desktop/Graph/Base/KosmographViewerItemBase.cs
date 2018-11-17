using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Kosmograph.Desktop.Graph.Base
{
    public abstract class KosmographViewerItemBase
    {
        /// <summary>
        /// A Kosmograoh viewer item is composed of multiple visual elements
        /// </summary>
        abstract public IEnumerable<FrameworkElement> FrameworkElements { get; }

        protected void UpdateVisibility(DrawingObject drawingObject)
        {
            foreach (var frameworkElement in this.FrameworkElements)
            {
                frameworkElement.Visibility = drawingObject.IsVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        #region A viewer element can participate in a dragging action on the canvas

        private bool markedForDragging;

        public bool MarkedForDragging
        {
            get => this.markedForDragging;
            set
            {
                this.markedForDragging = value;
                if (value)
                {
                    this.MarkedForDraggingEvent?.Invoke(this, null);
                }
                else
                {
                    this.UnmarkedForDraggingEvent?.Invoke(this, null);
                }
            }
        }

        public event EventHandler MarkedForDraggingEvent;

        public event EventHandler UnmarkedForDraggingEvent;

        #endregion A viewer element can participate in a dragging action on the canvas
    }
}