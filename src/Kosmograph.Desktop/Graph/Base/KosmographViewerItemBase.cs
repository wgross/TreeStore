using System.Collections.Generic;
using System.Windows;

namespace Kosmograph.Desktop.Graph.Base
{
    public abstract class KosmographViewerItemBase
    {
        /// <summary>
        /// A Kosmograoh viewer item is composed of multiple visual elements
        /// </summary>
        virtual public IEnumerable<FrameworkElement> FrameworkElements { get; }
    }
}