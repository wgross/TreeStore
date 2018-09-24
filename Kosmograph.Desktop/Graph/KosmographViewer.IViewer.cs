using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Windows;

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
        /// creates a viewer node
        /// </summary>
        /// <param name="drawingNode"></param>
        /// <returns></returns>
        public IViewerNode CreateIViewerNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            var frameworkElement = CreateTextBlockForDrawingObject(drawingNode);
            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;
            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);
            drawingNode.GeometryNode = new Microsoft.Msagl.Core.Layout.Node(bc, drawingNode);
            var vNode = CreateVNode(drawingNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);
            return vNode;
        }

        public IViewerNode CreateIViewerNode(Microsoft.Msagl.Drawing.Node drawingNode, Microsoft.Msagl.Core.Geometry.Point center, object visualElement)
        {
            if (this.drawingGraph == null)
                return null;

            var frameworkElement = visualElement as FrameworkElement ?? this.CreateTextBlockForDrawingObject(drawingNode);

            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;

            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve(drawingNode, width, height);

            drawingNode.GeometryNode = new Microsoft.Msagl.Core.Layout.Node(bc, drawingNode) { Center = center };

            var vNode = CreateVNode(drawingNode);
            drawingGraph.AddNode(drawingNode);
            drawingGraph.GeometryGraph.Nodes.Add(drawingNode.GeometryNode);
            layoutEditor.AttachLayoutChangeEvent(vNode);

            this.MakeRoomForNewNode(drawingNode);

            return vNode;
        }

        private void MakeRoomForNewNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            IncrementalDragger incrementalDragger = new IncrementalDragger(new[] { drawingNode.GeometryNode },
                                                                           Graph.GeometryGraph,
                                                                           Graph.LayoutAlgorithmSettings);
            incrementalDragger.Drag(new Microsoft.Msagl.Core.Geometry.Point());

            foreach (var n in incrementalDragger.ChangedGraph.Nodes)
            {
                var dn = (Microsoft.Msagl.Drawing.Node)n.UserData;
                var vn = drawingObjectsToIViewerObjects[dn] as VNode;
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
    }
}