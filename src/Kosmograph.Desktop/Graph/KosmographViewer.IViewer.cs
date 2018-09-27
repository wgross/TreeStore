using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using GeometryPoint = Microsoft.Msagl.Core.Geometry.Point;
using LayoutNode = Microsoft.Msagl.Core.Layout.Node;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer : IViewer
    {
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

        public IViewerNode CreateIViewerNode(DrawingNode drawingNode) => this.CreateIViewerNode(drawingNode, new GeometryPoint(10, 10), null);

        public IViewerNode CreateIViewerNode(DrawingNode drawingNode, GeometryPoint center, object visualElement)
        {
            

            var visualFrameworkElement = (visualElement as TextBlock) ?? VisualsFactory.CreateLabel(drawingNode.Label);

            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, visualFrameworkElement.Width + (2 * drawingNode.Attr.LabelMargin), visualFrameworkElement.Height + (2 * drawingNode.Attr.LabelMargin));

            drawingNode.GeometryNode = new LayoutNode(bc, drawingNode) { Center = center };

            var vNode = this.GetOrCreateViewerNode(drawingNode);

            this.Graph.AddNode(drawingNode);
            this.Graph.GeometryGraph.Nodes.Add(drawingNode.GeometryNode);
            this.layoutEditor.AttachLayoutChangeEvent(vNode);

            vNode.Invalidate();

            this.MakeRoomForNewNode(drawingNode);

            return vNode;
        }

        private void MakeRoomForNewNode(DrawingNode drawingNode)
        {
            IncrementalDragger incrementalDragger = new IncrementalDragger(new[] { drawingNode.GeometryNode },
                                                                           Graph.GeometryGraph,
                                                                           Graph.LayoutAlgorithmSettings);
            incrementalDragger.Drag(new GeometryPoint());

            foreach (var n in incrementalDragger.ChangedGraph.Nodes)
            {
                var dn = (DrawingNode)n.UserData;
                var vn = drawingObjectsToIViewerObjects[dn] as KosmographViewerNode;
                if (vn != null)
                    vn.Invalidate();
            }

            foreach (var n in incrementalDragger.ChangedGraph.Edges)
            {
                var dn = (Microsoft.Msagl.Drawing.Edge)n.UserData;
                var ve = drawingObjectsToIViewerObjects[dn] as VEdge;
                if (ve != null)
                    ve.Invalidate();
            }
        }

        public void Invalidate(IViewerObject objectToInvalidate) => ((IInvalidatable)objectToInvalidate).Invalidate();
    }
}