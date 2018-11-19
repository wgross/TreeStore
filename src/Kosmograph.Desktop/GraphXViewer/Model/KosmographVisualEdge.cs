using GraphX.PCL.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosmograph.Desktop.GraphXViewer.Model
{
    public sealed class KosmographVisualEdge : EdgeBase<KosmographVisualVertex>
    {
        public KosmographVisualEdge(KosmographVisualVertex source, KosmographVisualVertex target, double weight = 1)
            : base(source, target, weight)
        {}
    }
}
