using Kosmograph.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class GraphXViewerViewModel
    {
        private readonly ObservableCollection<Entity> entities = new ObservableCollection<Entity>();

        public IEnumerable<Entity> Entities => entities;

        public void Show(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                this.entities.Add(entity);
        }

        private readonly ObservableCollection<Relationship> relationships = new ObservableCollection<Relationship>();

        public IEnumerable<Relationship> Relationships => relationships;

        public void Show(IEnumerable<Relationship> relationships)
        {
            foreach (var relationship in relationships)
                this.relationships.Add(relationship);
        }
    }
}