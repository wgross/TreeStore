using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using GeometryPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer : IViewer
    {
        public event EventHandler GraphChanged;

        public Microsoft.Msagl.Drawing.Graph Graph
        {
            get => this.drawingGraph;
            set
            {
                this.drawingGraph = value;
                if (this.drawingGraph != null)
                    Console.WriteLine("starting processing a graph with {0} nodes and {1} edges", drawingGraph.NodeCount, drawingGraph.EdgeCount);
                this.FillKosmographViewer();
            }
        }

        private Microsoft.Msagl.Drawing.Graph drawingGraph;

        private GeometryGraph GeometryGraph => this.drawingGraph.GeometryGraph;

        /// <summary>
        /// Keep all created IViewr objects for later lookup
        /// </summary>
        private readonly Dictionary<DrawingObject, IViewerObject> drawingObjectsToIViewerObjects = new Dictionary<DrawingObject, IViewerObject>();

        public IViewerNode CreateIViewerNode(DrawingNode drawingNode)
            => this.AddViewerNode(drawingNode);

        public IViewerNode CreateIViewerNode(DrawingNode drawingNode, GeometryPoint center, object visualElement)
            => this.AddViewerNode(drawingNode);

        public void Invalidate(IViewerObject objectToInvalidate) => ((IInvalidatable)objectToInvalidate).Invalidate();

        public void Invalidate()
        {
            //todo: is it right to do nothing
        }
    }
}