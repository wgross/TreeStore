using Kosmograph.Desktop.ViewModel;
using System.Windows;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographControl
    {
        public KosmographViewModel ViewModel => (KosmographViewModel)this.DataContext;

        private void KosmographControl_Loaded(object sender, RoutedEventArgs e)
        {
            var graph = new Microsoft.Msagl.Drawing.Graph();

            foreach (var entity in this.ViewModel.Entities)
            {
                var node = graph.AddNode(entity.Model.Id.ToString());
                node.LabelText = entity.Name;
                node.Attr.LineWidth = 1;
                node.Attr.XRadius = 0;
                node.Attr.YRadius = 0;
            }

            foreach (var relationship in this.ViewModel.Relationships)
            {
                var edge = graph.AddEdge(relationship.From.Model.Id.ToString(), relationship.To.Model.Id.ToString());
                edge.LabelText = relationship.Name;
            }
            graph.Attr.LayerDirection = Microsoft.Msagl.Drawing.LayerDirection.LR;
            this.msaglGraphViewer.Graph = graph;
        }
    }
}