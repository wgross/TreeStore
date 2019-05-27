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
            this.graphArea.GenerateGraph(true, true);
            this.graphArea.SetEdgesDashStyle(EdgeDashStyle.Solid);
            this.graphArea.ShowAllEdgesArrows(false);
            this.graphArea.ShowAllEdgesLabels(true);
            this.graphArea.SetVerticesDrag(true);
        }

        private GraphViewModel SetupGraph()
        {
            //Lets make new data graph instance

            this.ViewModel.GraphCallback = this;
            var visualModel = this.ViewModel.GraphViewModel;

            //Now we need to create edges and vertices to fill data graph
            //This edges and vertices will represent graph structure and connections
            //Lets make some vertices
            //foreach (var entity in this.ViewModel.Entities)
            //{
            //    this.ViewModel.GraphViewModel.AddVertex(new VertexViewModel()
            //    {
            //        ModelId = entity.Id,
            //        Label = entity.Name
            //    });
            //}

            //var vertices = visualModel.Vertices.ToDictionary(v => v.ModelId);

            //foreach (var edge in this.ViewModel.Relationships)
            //{
            //    var visualEdge = new EdgeViewModel(vertices[edge.From.Id], vertices[edge.To.Id])
            //    {
            //        ModelId = edge.Id,
            //        Label = edge.Name
            //    };
            //    visualModel.AddEdge(visualEdge);
            //}

            return visualModel;
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
                this.graphArea.AddVertexAndData(vertexData, this.graphArea.ControlFactory.CreateVertexControl(vertexData));
                this.graphArea.RelayoutGraph(true);
                this.zoomctrl.ZoomToFill();
            });
        }

        public void Remove(VertexViewModel vertex)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.graphArea.RemoveVertexAndEdges(vertex);
                this.graphArea.RelayoutGraph(true);
                this.zoomctrl.ZoomToFill();
            });
        }

        public void Add(EdgeViewModel edge)
        {
            this.Dispatcher.Invoke(() =>
            {
                var sourceVertex = this.graphArea.VertexList[edge.Source];
                var targetVertax = this.graphArea.VertexList[edge.Target];

                this.graphArea.AddEdgeAndData(edge, this.graphArea.ControlFactory.CreateEdgeControl(sourceVertex, targetVertax, edge), generateLabel: true);
            });
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
    }
}