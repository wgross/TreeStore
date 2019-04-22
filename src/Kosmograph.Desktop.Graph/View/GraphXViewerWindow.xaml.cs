using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows;

namespace Kosmograph.Desktop.Graph.View
{
    /// <summary>
    /// Interaction logic for GraphXViewerWindow.xaml
    /// </summary>
    public partial class GraphXViewerWindow : Window, IDisposable
    {
        public GraphXViewerViewModel ViewModel => (GraphXViewerViewModel)this.DataContext;

        public GraphXViewerWindow()
        {
            InitializeComponent();
            // Messenger.Default.Register<EditModelCommitted>(this, this.EditModelCommitted);
            ZoomControl.SetViewFinderVisibility(this.zoomctrl, Visibility.Visible);

            this.Loaded += this.GraphXViewerWindow_Loaded;
        }

        private void GraphXViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetupGraphArea();

            this.graphArea.GenerateGraph(true, true);
            this.graphArea.SetEdgesDashStyle(EdgeDashStyle.Solid);
            this.graphArea.ShowAllEdgesArrows(false);
            this.graphArea.ShowAllEdgesLabels(true);
            this.graphArea.SetVerticesDrag(true);

            this.zoomctrl.ZoomToFill();
        }

        #region Setup graph from view model

        private void SetupGraphArea()
        {
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new GraphXGraphLogic()
            {
                Graph = this.SetupGraph()
            };
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.AsyncAlgorithmCompute = false;

            this.graphArea.LogicCore = logicCore;
        }

        private GraphViewModel SetupGraph()
        {
            //Lets make new data graph instance
            var visualModel = new GraphViewModel();
            //Now we need to create edges and vertices to fill data graph
            //This edges and vertices will represent graph structure and connections
            //Lets make some vertices
            foreach (var entity in this.ViewModel.Entities)
            {
                visualModel.AddVertex(new VertexViewModel()
                {
                    ModelId = entity.Id,
                    Label = entity.Name
                });
            }

            var vertices = visualModel.Vertices.ToDictionary(v => v.ModelId);

            foreach (var edge in this.ViewModel.Relationships)
            {
                var visualEdge = new EdgeViewModel(vertices[edge.From.Id], vertices[edge.To.Id])
                {
                    ModelId = edge.Id,
                    Label = edge.Name
                };
                visualModel.AddEdge(visualEdge);
            }

            return visualModel;
        }

        private static void AddEdge(GraphViewModel visualModel, VertexViewModel fromVertex, VertexViewModel toVertex, Relationship edge)
        {
        }

        #endregion Setup graph from view model

        public void Dispose()
        {
            this.graphArea.Dispose();
        }

        #region Update Graph from view model

        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.Property.Equals(DataContextProperty))
        //    {
        //        if ((e.OldValue as KosmographViewModel) != null)
        //        {
        //            ((KosmographViewModel)e.OldValue).Relationships.CollectionChanged -= this.Relationships_CollectionChanged;
        //            ((KosmographViewModel)e.OldValue).Entities.CollectionChanged -= this.Entities_CollectionChanged;
        //        }

        //        //if ((e.NewValue as KosmographViewModel) != null)
        //        //{
        //        //    ((KosmographViewModel)e.NewValue).Relationships.CollectionChanged += this.Relationships_CollectionChanged;
        //        //    ((KosmographViewModel)e.NewValue).Entities.CollectionChanged += this.Entities_CollectionChanged;
        //        //}
        //    }
        //    base.OnPropertyChanged(e);
        //}

        //private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
        //            var data = new KosmographVisualVertexModel()
        //            {
        //                ModelId = e.NewItems.OfType<EntityViewModel>().Single().Model.Id,
        //                Label = e.NewItems.OfType<EntityViewModel>().Single().Name
        //            };
        //            this.graphArea.AddVertexAndData(data, new VertexControl(data), true);
        //            this.graphArea.RelayoutGraph(true);
        //            break;

        //        case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
        //            this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualVertexModel>(e.OldItems.OfType<EntityViewModel>().Single().Model.Id, out var vertexData);
        //            this.graphArea.RemoveVertexAndEdges(vertexData);
        //            this.graphArea.RelayoutGraph(true);
        //            break;
        //    }
        //}

        //private void Relationships_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    // Only adding ai handled here.
        //    // removelö of edges is trigger by removal of one of the nodes of the edge.
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            {
        //                var relationshipToAdd = e.NewItems.OfType<RelationshipViewModel>().Single();
        //                if (this.TryGetNode(relationshipToAdd.Model.From.Id, out var fromVertex))
        //                    if (this.TryGetNode(relationshipToAdd.Model.To.Id, out var toVertex))
        //                    {
        //                        var edgeModel = new KosmographVisualEdgeModel(fromVertex, toVertex)
        //                        {
        //                            ModelId = relationshipToAdd.Model.Id,
        //                            Label = relationshipToAdd.Name
        //                        };

        //                        this.graphArea.AddEdgeAndData(edgeModel, new EdgeControl(this.graphArea.VertexList[fromVertex], this.graphArea.VertexList[toVertex], edgeModel), generateLabel: true);
        //                    }
        //                break;
        //            }

        //        case NotifyCollectionChangedAction.Remove:
        //            {
        //                var relationshipToRemove = e.OldItems.OfType<RelationshipViewModel>().Single();
        //                if (this.TryGetEdge(relationshipToRemove, out var edgeToRemove))
        //                    this.graphArea.RemoveEdge(edgeToRemove, removeEdgeFromDataGraph: true);
        //                break;
        //            }
        //    }
        //}

        //private void EditModelCommitted(EditModelCommitted notification)
        //{
        //    var (isEntity, entity) = notification.TryGetViewModel<EntityViewModel>();
        //    if (isEntity)
        //    {
        //        this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualVertexModel>(entity.Model.Id, out var visualVertexModel);
        //        visualVertexModel.Label = entity.Name;
        //    }

        //    var (isRelationship, relationship) = notification.TryGetViewModel<RelationshipViewModel>();
        //    if (isRelationship)
        //    {
        //        if (this.TryGetEdge(relationship, out var edge))
        //            edge.Label = relationship.Name;
        //    }
        //}

        //private bool TryGetNode(Guid nodeId, out KosmographVisualVertexModel vertex) => this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualVertexModel>(nodeId, out vertex);

        //private (bool, KosmographVisualVertexModel, KosmographVisualVertexModel) TryGetNodes(RelationshipViewModel relationship)
        //{
        //    if (this.TryGetNode(relationship.From.Model.Id, out var visualSourceVertexModel))
        //        if (this.TryGetNode(relationship.To.Model.Id, out var visualTargetVertexModel))
        //            return (true, visualSourceVertexModel, visualTargetVertexModel);
        //    return (false, null, null);
        //}

        //private bool TryGetEdge(RelationshipViewModel relationship, out KosmographVisualEdgeModel edge)
        //{
        //    var (edgeVerticesFound, fromVertex, toVertex) = TryGetNodes(relationship);
        //    if (edgeVerticesFound)
        //        if (this.graphArea.LogicCore.Graph.TryGetEdge(fromVertex, toVertex, out edge))
        //            return true;
        //    edge = null;
        //    return false;
        //}
    }

    #endregion Update Graph from view model
}