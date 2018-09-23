using Kosmograph.Desktop.ViewModel;
using System.Windows;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographControl
    {
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

        private void Entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void Relationships_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void AddKosmographNodesAndEdges(KosmographViewModel viewModel)
        {
            var graph = new Microsoft.Msagl.Drawing.Graph();

            foreach (var entity in viewModel.Entities)
            {
                var node = graph.AddNode(entity.Model.Id.ToString());
                node.LabelText = entity.Name;
                node.Attr.LineWidth = 1;
                node.Attr.XRadius = 0;
                node.Attr.YRadius = 0;
            }

            foreach (var relationship in viewModel.Relationships)
            {
                var edge = graph.AddEdge(relationship.From.Model.Id.ToString(), relationship.To.Model.Id.ToString());
                edge.LabelText = relationship.Name;
            }
            graph.Attr.LayerDirection = Microsoft.Msagl.Drawing.LayerDirection.LR;
            this.msaglGraphViewer.Graph = graph;
        }
    }
}