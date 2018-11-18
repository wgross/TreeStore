using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Kosmograph.Desktop.MsaglGraph
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
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    this.MsaglGraphViewer.AddNode(e.NewItems.OfType<EntityViewModel>().Single());
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    this.MsaglGraphViewer.RemoveNode(e.OldItems.OfType<EntityViewModel>().Single());
                    break;
            }
        }

        private void Relationships_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void EditModelCommitted(EditModelCommitted notification)
        {
            var (isEntity, entity) = notification.TryGetViewModel<EntityViewModel>();
            if (isEntity)
                this.MsaglGraphViewer.UpdateNode(entity);
            var (isRelationship, relationship) = notification.TryGetViewModel<RelationshipViewModel>();
            if(isRelationship)
                this.MsaglGraphViewer.UpdateEdge(relationship);
        }
        
        private void AddKosmographNodesAndEdges(KosmographViewModel viewModel)
        {
            var graph = new Microsoft.Msagl.Drawing.Graph();

            this.AddEntities(viewModel.Entities, graph);

            foreach (var relationship in viewModel.Relationships)
            {
                var edge = graph.AddEdge(relationship.From.Model.Id.ToString(), relationship.To.Model.Id.ToString());
                edge.LabelText = relationship.Name;
            }
            graph.Attr.LayerDirection = Microsoft.Msagl.Drawing.LayerDirection.LR;
            this.MsaglGraphViewer.Graph = graph;
        }

        private void AddEntities(IEnumerable<EntityViewModel> entities, Microsoft.Msagl.Drawing.Graph graph)
        {
            foreach (var entity in entities)
            {
                var node = graph.AddNode(entity.Model.Id.ToString());
                node.LabelText = entity.Name;
                node.Attr.LineWidth = 1;
                node.Attr.XRadius = 0;
                node.Attr.YRadius = 0;
            }
        }
    }
}