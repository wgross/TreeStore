using Microsoft.Msagl.Drawing;
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
    }
}