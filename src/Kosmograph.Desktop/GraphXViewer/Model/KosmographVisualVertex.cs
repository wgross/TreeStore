using GraphX.PCL.Common.Models;
using System;

namespace Kosmograph.Desktop.GraphXViewer.Model
{
    public class KosmographVisualVertex : VertexBase
    {
        public KosmographVisualVertex()
        {
        }

        public string Label { get; set; }

        public Guid ModelId { get; set; }

        public override string ToString() => this.Label;
    }
}