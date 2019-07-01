using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph.View
{
    /// <summary>
    /// Interaction logic for GraphXViewer.xaml
    /// </summary>
    public partial class GraphXViewer : UserControl, IDisposable, IGraphCallback
    {
        public GraphXViewerViewModel ViewModel => (GraphXViewerViewModel)this.DataContext;

        public GraphXViewer()
        {
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(GraphXViewerCommands.ClearCommand, this.ClearCommandExecuted));
            ZoomControl.SetViewFinderVisibility(this.zoomctrl, Visibility.Visible);
        }

        private void ClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.graphArea.ClearLayout(true, true, true);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Equals(FrameworkElement.DataContextProperty))
            {
                if (e.NewValue is null)
                    return;

                this.SetupGraphArea();
                this.zoomctrl.ZoomToFill();
            }
        }

        #region Setup graph from view model

        private void SetupGraphArea()
        {
            //Lets create logic core and filled data graph with edges and vertices
            var logicCore = new GraphXGraphLogic()
            {
                Graph = this.SetupGrapViewModel()
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
            this.graphArea.GenerateGraph(true, true);
            this.graphArea.SetEdgesDashStyle(EdgeDashStyle.Solid);
            this.graphArea.ShowAllEdgesArrows(false);
            this.graphArea.ShowAllEdgesLabels(true);
            this.graphArea.SetVerticesDrag(true);
        }

        private GraphViewModel SetupGrapViewModel()
        {
            this.ViewModel.GraphCallback = this;
            return this.ViewModel.GraphViewModel;
        }

        #endregion Setup graph from view model

        public void Dispose()
        {
            this.graphArea.Dispose();
        }

        #region IGraphCallback Members

        public void Add(VertexViewModel vertexData)
        {
            this.Dispatcher.Invoke(() =>
            {
                var vertexControl = this.graphArea.ControlFactory.CreateVertexControl(vertexData);
                vertexControl.EventOptions.MouseDoubleClickEnabled = true;
                vertexControl.MouseDoubleClick += this.VertexControl_MouseDoubleClick;
                this.graphArea.AddVertexAndData(vertexData, vertexControl);
                this.graphArea.RelayoutGraph(true);
                this.zoomctrl.ZoomToFill();
            });
        }

        private void VertexControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.RaiseEditEntityEvent(sender.AsType<VertexControl>()?.Vertex.AsType<VertexViewModel>()?.ModelId);
        }

        public void Remove(VertexViewModel vertex)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.graphArea.RemoveVertexAndEdges(vertex);
                this.RelayoutGraphSafe();
                this.zoomctrl.ZoomToFill();
            });
        }

        private void RelayoutGraphSafe()
        {
            if (this.ViewModel.GraphViewModel.VertexCount == 1)
                return; // there seems to be a bug if only one vertex remains.
            this.graphArea.RelayoutGraph(true);
        }

        public void Add(EdgeViewModel edge)
        {
            this.Dispatcher.Invoke(() =>
            {
                var sourceVertex = this.graphArea.VertexList[edge.Source];
                var targetVertax = this.graphArea.VertexList[edge.Target];
                var edgeControl = this.graphArea.ControlFactory.CreateEdgeControl(sourceVertex, targetVertax, edge);
                edgeControl.EventOptions.MouseClickEnabled = true;
                edgeControl.MouseDoubleClick += this.EdgeControl_MouseDoubleClick;

                this.graphArea.AddEdgeAndData(edge, this.graphArea.ControlFactory.CreateEdgeControl(sourceVertex, targetVertax, edge), generateLabel: true);
            });
        }

        private void EdgeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.RaiseEditRelationshipEvent(sender.AsType<EdgeControl>()?.Edge.AsType<EdgeViewModel>()?.ModelId);
        }

        public void Remove(EdgeViewModel edge)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.graphArea.RemoveEdge(edge);
                this.graphArea.RelayoutGraph(true);
                this.zoomctrl.ZoomToFill();
            });
        }

        #endregion IGraphCallback Members

        #region Drag&Drop

        private void graphXViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Tag)))
            {
                this.ViewModel.ShowTag((Tag)e.Data.GetData(typeof(Tag)));
            }
            e.Handled = true;
        }

        #endregion Drag&Drop

        #region Request editing of an entity

        public static readonly RoutedEvent EditEntityEvent = EventManager.RegisterRoutedEvent(nameof(EditEntity), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GraphXViewer));

        public event RoutedEventHandler EditEntity
        {
            add
            {
                this.AddHandler(EditEntityEvent, value);
            }
            remove
            {
                this.RemoveHandler(EditEntityEvent, value);
            }
        }

        private void RaiseEditEntityEvent(Guid? entityId)
        {
            if (entityId != null)
                this.RaiseEvent(new EditEntityByIdRoutedEventArgs(EditEntityEvent, entityId.Value));
        }

        #endregion Request editing of an entity

        #region Request editing of a relationship

        public static readonly RoutedEvent EditRelationshipEvent = EventManager.RegisterRoutedEvent(nameof(EditRelationship), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GraphXViewer));

        public event RoutedEventHandler EditRelationship
        {
            add
            {
                this.AddHandler(EditRelationshipEvent, value);
            }
            remove
            {
                this.RemoveHandler(EditRelationshipEvent, value);
            }
        }

        private void RaiseEditRelationshipEvent(Guid? relationshipId)
        {
            if (relationshipId != null)
                this.RaiseEvent(new EditRelationshipByIdRoutedEventArgs(EditRelationshipEvent, relationshipId.Value));
        }

        #endregion Request editing of a relationship
    }
}