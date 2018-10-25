﻿using Kosmograph.Desktop.ViewModel;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrawingNode = Microsoft.Msagl.Drawing.Node;
using GeometryPoint = Microsoft.Msagl.Core.Geometry.Point;
using GeometryRectangle = Microsoft.Msagl.Core.Geometry.Rectangle;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        private const double DesiredPathThicknessInInches = 0.008;

        private double GetBorderPathThickness() => DesiredPathThicknessInInches * DpiX;

        #region Create of viewer nodes during fill phase

        private void CreateViewerNodes()
        {
            foreach (var node in this.Graph.Nodes.Concat(this.Graph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf()))
                this.CreateViewerNode(node);
        }

        private KosmographViewerNode CreateViewerNode(DrawingNode drawingNode)
        {
            if (drawingNode.GeometryNode is null)
            {
                // make sure the drawning node has a gemoetry node.
                // this is true in the inietial 'fill' phase bit if the node is added later
                // the DrawingNode is not yet completed
                GeometryGraphCreator.CreateGeometryNode(this.Graph, this.GeometryGraph, drawingNode, ConnectionToGraph.Connected);
                this.Graph.GeometryGraph.Nodes.Add(drawingNode.GeometryNode);
            }

            var viewerNode = new KosmographViewerNode(drawingNode, VisualsFactory.CreateLabel(drawingNode.Label), funcFromDrawingEdgeToVEdge: e => (KosmographViewerEdge)drawingObjectsToIViewerObjects[e])
            {
                PathStrokeThicknessFunc = () => GetBorderPathThickness() * drawingNode.Attr.LineWidth
            };

            this.drawingObjectsToIViewerObjects[drawingNode] = viewerNode;

            return viewerNode;
        }

        private IEnumerable<KosmographViewerNode> GetViewerNodes()
        {
            foreach (var node in this.Graph.Nodes.Concat(this.Graph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf()))
            {
                yield return this.GetViewerNode(node);
            }
        }

        private KosmographViewerNode GetViewerNode(DrawingNode drawingNode)
        {
            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingNode, out var existingViewerNode))
                return (KosmographViewerNode)existingViewerNode;
            return null;
        }

        #endregion Create of viewer nodes during fill phase

        #region Create (add) viewer nodes to an already displayed graph

        public void AddNode(EntityViewModel node)
        {
            var drawingNode = this.Graph.AddNode(node.Model.Id.ToString());
            drawingNode.Label.Text = node.Name;
            // lateron the view model might define graphical attributes itself
            drawingNode.Attr.LineWidth = 1;
            drawingNode.Attr.XRadius = 0;
            drawingNode.Attr.YRadius = 0;

            this.AddViewerNode(drawingNode);

            //adds all nodes again->refactor//this.RunLayoutInUIThread();
        }

        private KosmographViewerNode AddViewerNode(DrawingNode drawingNode) //dont know how to place it better//, GeometryPoint center, object visualElement)
        {
            var viewerNode = this.CreateViewerNode(drawingNode);

            this.LayoutEditor.AttachLayoutChangeEvent(viewerNode);

            this.GraphCanvasAddChildren(viewerNode.FrameworkElements);

            this.MakeRoomForNewNode(drawingNode);

            return viewerNode;
        }

        private void MakeRoomForNewNode(DrawingNode drawingNode)
        {
            var incrementalDragger =
                new IncrementalDragger(new[] { drawingNode.GeometryNode }, this.Graph.GeometryGraph, this.Graph.LayoutAlgorithmSettings);

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
                var ve = drawingObjectsToIViewerObjects[dn] as KosmographViewerEdge;
                if (ve != null)
                    ve.Invalidate();
            }
        }

        #endregion Create (add) viewer nodes to an already displayed graph

        #region Update a viewer node with modified data

        public void UpdateNode(EntityViewModel node)
        {
            var drawingNode = this.Graph.FindNode(node.Model.Id.ToString());
            if (drawingNode is null)
                return;

            // update the underlying label
            drawingNode.Label.Text = node.Name;

            // remasure the node
            var tb = new TextBlock { Text = node.Name };
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            drawingNode.GeometryNode.BoundingBox = new GeometryRectangle(0, 0, tb.DesiredSize.Width, tb.DesiredSize.Height)
            {
                Center = drawingNode.GeometryNode.BoundingBox.Center
            };

            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingNode, out var nodeViewer))
                this.Invalidate(nodeViewer);
        }

        #endregion Update a viewer node with modified data

        #region Remove a Node

        public void RemoveNode(EntityViewModel node)
        {
            var drawingNode = this.Graph.FindNode(node.Model.Id.ToString());
            if (drawingNode is null)
                return;

            // remove the node
            if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingNode, out var viewerNode))
                this.GraphCanvasRemoveChildren(((KosmographViewerNode)viewerNode).FrameworkElements);
            this.drawingObjectsToIViewerObjects.Remove(drawingNode);

            // removes the nodes edges
            foreach (var drawingEdge in drawingNode.Edges)
            {
                if (this.drawingObjectsToIViewerObjects.TryGetValue(drawingEdge, out var viewerEdge))
                {
                    this.GraphCanvasRemoveChildren(((KosmographViewerEdge)viewerEdge).FrameworkElements);
                    this.drawingObjectsToIViewerObjects.Remove(drawingEdge);
                }
            }

            // remove the node from te underlying MSAGL graph.
            // this will incliude the edges
            this.Graph.RemoveNode(drawingNode);
        }

        #endregion Remove a Node
    }
}