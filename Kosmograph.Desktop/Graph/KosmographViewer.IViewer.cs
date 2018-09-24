using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using Microsoft.Msagl.WpfGraphControl;
using System.Windows;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        /// <summary>
        /// creates a viewer node
        /// </summary>
        /// <param name="drawingNode"></param>
        /// <returns></returns>
        public IViewerNode CreateIViewerNode(Microsoft.Msagl.Drawing.Node drawingNode)
        {
            var frameworkElement = CreateTextBlockForMsaglDrawingObject(drawingNode);
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

            var frameworkElement = visualElement as FrameworkElement ?? this.CreateTextBlockForMsaglDrawingObject(drawingNode);

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