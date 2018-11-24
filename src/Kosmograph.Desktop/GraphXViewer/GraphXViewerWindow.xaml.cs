using GalaSoft.MvvmLight.Messaging;
using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.GraphXViewer.Model;
using Kosmograph.Desktop.ViewModel;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace Kosmograph.Desktop.GraphXViewer
{
    /// <summary>
    /// Interaction logic for GraphXViewerWindow.xaml
    /// </summary>
    public partial class GraphXViewerWindow : Window, IDisposable
    {
        public KosmographViewModel ViewModel
        {
            get
            {
                return (KosmographViewModel)this.DataContext;
            }
        }

        public GraphXViewerWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<EditModelCommitted>(this, this.EditModelCommitted);
            ZoomControl.SetViewFinderVisibility(this.zoomctrl, Visibility.Visible);

            this.Loaded += this.GraphXViewerWindow_Loaded;
        }

        private void EditModelCommitted(EditModelCommitted notification)
        {
            var (isEntity, entity) = notification.TryGetViewModel<EntityViewModel>();
            if (isEntity)
            {
                this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualVertexModel>(entity.Model.Id, out var visualVertexModel);
                visualVertexModel.Label = entity.Name;
            }
            
            var (isRelationship, relationship) = notification.TryGetViewModel<RelationshipViewModel>();
            if (isRelationship)
            {
                this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualEdgeModel>(relationship.Model.Id, out var visualEdgeModel);
                visualEdgeModel.Label = relationship.Name;
            }
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

        private void SetupGraphArea()
        {
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new KosmographVisualGraphLogic()
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

        private KosmographVisualGraphModel SetupGraph()
        {
            //Lets make new data graph instance
            var visualModel = new KosmographVisualGraphModel();
            //Now we need to create edges and vertices to fill data graph
            //This edges and vertices will represent graph structure and connections
            //Lets make some vertices
            foreach (var entity in this.ViewModel.Entities)
            {
                visualModel.AddVertex(new KosmographVisualVertexModel()
                {
                    ModelId = entity.Model.Id,
                    Label = entity.Name
                });
            }

            var vertices = visualModel.Vertices.ToDictionary(v => v.ModelId);

            foreach (var edge in this.ViewModel.Relationships)
            {
                var visualEdge = new KosmographVisualEdgeModel(vertices[edge.Model.From.Id], vertices[edge.Model.To.Id])
                {
                    ModelId = edge.Model.Id,
                    Label = edge.Name
                };
                visualModel.AddEdge(visualEdge);
            }

            return visualModel;
        }

        public void Dispose()
        {
            this.graphArea.Dispose();
        }

        #region Update Graph from view model

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Equals(DataContextProperty))
            {
                if ((e.OldValue as KosmographViewModel) != null)
                {
                    ((KosmographViewModel)e.OldValue).Relationships.CollectionChanged -= this.Relationships_CollectionChanged;
                    ((KosmographViewModel)e.OldValue).Entities.CollectionChanged -= this.Entities_CollectionChanged;
                }

                if ((e.NewValue as KosmographViewModel) != null)
                {
                    ((KosmographViewModel)e.NewValue).Relationships.CollectionChanged += this.Relationships_CollectionChanged;
                    ((KosmographViewModel)e.NewValue).Entities.CollectionChanged += this.Entities_CollectionChanged;
                }
            }
            base.OnPropertyChanged(e);
        }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var data = new KosmographVisualVertexModel()
                    {
                        ModelId = e.NewItems.OfType<EntityViewModel>().Single().Model.Id,
                        Label = e.NewItems.OfType<EntityViewModel>().Single().Name
                    };
                    this.graphArea.AddVertexAndData(data, new VertexControl(data), true);
                    this.graphArea.RelayoutGraph(true);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    this.graphArea.LogicCore.Graph.IdentityMap.TryGetTarget<KosmographVisualVertexModel>(e.OldItems.OfType<EntityViewModel>().Single().Model.Id, out var vertexData);
                    this.graphArea.RemoveVertexAndEdges(vertexData);
                    this.graphArea.RelayoutGraph(true);
                    break;
            }
        }

        private void Relationships_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    #endregion Update Graph from view model
}