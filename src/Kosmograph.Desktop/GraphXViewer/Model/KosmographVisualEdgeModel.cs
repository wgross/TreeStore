using GraphX.PCL.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosmograph.Desktop.GraphXViewer.Model
{
    public sealed class KosmographVisualEdgeModel : EdgeBase<KosmographVisualVertexModel>
    {
        public KosmographVisualEdgeModel(KosmographVisualVertexModel source, KosmographVisualVertexModel target, double weight = 1)
            : base(source, target, weight)
        {}

        public string Label { get; set; }

        public override string ToString() => this.Label;
    }
}
