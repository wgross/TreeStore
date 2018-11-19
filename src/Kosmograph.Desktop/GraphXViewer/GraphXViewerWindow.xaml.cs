using GraphX.Controls;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using Kosmograph.Desktop.GraphXViewer.Model;
using Kosmograph.Desktop.ViewModel;
using System;
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

        private KosmographVisualModel SetupGraph()
        {
            //Lets make new data graph instance
            var visualModel = new KosmographVisualModel();
            //Now we need to create edges and vertices to fill data graph
            //This edges and vertices will represent graph structure and connections
            //Lets make some vertices
            foreach (var entity in this.ViewModel.Entities)
            {
                visualModel.AddVertex(new KosmographVisualVertex()
                {
                    ModelId = entity.Model.Id,
                    Label = entity.Name
                });
            }

            var vertices = visualModel.Vertices.ToDictionary(v => v.ModelId);

            foreach (var edge in this.ViewModel.Relationships)
            {
                var visualEdge = new KosmographVisualEdge(vertices[edge.Model.From.Id], vertices[edge.Model.To.Id])
                {
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
    }
}