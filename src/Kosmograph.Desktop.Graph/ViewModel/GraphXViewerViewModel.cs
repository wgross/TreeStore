using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class GraphXViewerViewModel : IObserver<ChangedMessage<IEntity>>, IObserver<ChangedMessage<IRelationship>>
    {
        private readonly KosmographModel model;
        private readonly IKosmographMessageBus messageBus;
        private readonly ModelController modelController;

        //private readonly IDisposable entitySubscription;
        //private readonly IDisposable relationshipSubscription;
        private readonly MultiTagQuery multiTagQuery;

        public GraphXViewerViewModel(KosmographModel model, IKosmographMessageBus messageBus)
        {
            this.model = model;
            this.messageBus = messageBus;
            //this.entitySubscription = this.messageBus.Entities.Subscribe(this);
            //this.relationshipSubscription = this.messageBus.Relationships.Subscribe(this);
            // model conreoller observes changes of model items
            this.modelController = new ModelController(this.messageBus);
            this.modelController.EntityChanged = this.OnEntityChanging;
            this.modelController.RelationshipChanged = this.OnRelationshipChanging;
            // multi tag query is the source of entities and relationships added to the graph
            this.multiTagQuery = new MultiTagQuery(this.model, this.messageBus);
            this.multiTagQuery.EntityAdded = this.OnEntityAdding;
            this.multiTagQuery.EntityRemoved = this.OnEntityRemoving;
            this.multiTagQuery.RelationshipAdded = this.OnRelationshipAdding;
            this.multiTagQuery.RelationshipRemoved = this.OnRelationshipRemoving;
        }

        public ObservableCollection<TagQueryViewModel> TagQueries { get; } = new ObservableCollection<TagQueryViewModel>();

        #region Build graph from tag queries

        public void ShowTag(Tag tag)
        {
            //var tagQuery = new TagQuery(this.model, this.messageBus, tag);
            //tagQuery.EntityAdded = e => this.GetOrAddEntity(e);
            //tagQuery.RelationshipAdded = this.AddRelationship;
            //tagQuery.StartQuery();

            this.TagQueries.Add(new TagQueryViewModel(this.multiTagQuery.Add(tag)));
        }

        #endregion Build graph from tag queries

        #region Handle tag multi query changes

        private void OnRelationshipRemoving(Guid relationshipId) => this.RemoveRelationship(relationshipId);

        private void OnRelationshipAdding(Relationship added) => this.AddRelationship(added);

        private void OnEntityRemoving(Guid obj) => this.RemoveEntity(obj);

        private void OnEntityAdding(Entity added) => this.GetOrAddEntity(added);

        #endregion Handle tag multi query changes

        #region Handle model item changes

        private void OnEntityChanging(Entity obj)
        {
        }

        private void OnRelationshipChanging(Relationship obj)
        {
        }

        #endregion Handle model item changes

        #region Manipulate the graph content consistently

        private VertexViewModel NewVertexViewModel(Entity e) => new VertexViewModel
        {
            Label = e.Name,
            ModelId = e.Id,
        };

        private VertexViewModel ExistingVertexViewModel(Func<VertexViewModel, bool> firstOrDefault) => this.GraphViewModel.Vertices.FirstOrDefault(firstOrDefault);

        private VertexViewModel ExistingVertexViewModel(Entity entity) => ExistingVertexViewModel(v => v.ModelId.Equals(entity.Id));

        private VertexViewModel GetOrAddEntity(Entity added)
        {
            var existing = ExistingVertexViewModel(added);
            if (existing is null)
                this.GraphCallback.Add(existing = NewVertexViewModel(added));
            return existing;
        }

        private void RemoveEntity(Guid entityId)
        {
            var existing = ExistingVertexViewModel(v => v.ModelId.Equals(entityId));
            if (existing is null)
                return;

            this.GraphCallback.Remove(existing);
        }

        private void AddRelationship(Relationship relationship)
        {
            var existing = ExistingEdgeViewModel(relationship);
            if (existing is null)
                this.GraphCallback.Add(this.NewEdgeViewModel(relationship, this.GetOrAddEntity(relationship.From), this.GetOrAddEntity(relationship.To)));
        }

        private void RemoveRelationship(Guid relationshipId)
        {
            var existing = ExistingEdgeViewModel(e => e.ModelId.Equals(relationshipId));
            if (existing is null)
                return;
            this.GraphCallback.Remove(existing);
        }

        private EdgeViewModel NewEdgeViewModel(Relationship relationship, VertexViewModel source, VertexViewModel target) => new EdgeViewModel(source, target)
        {
            ModelId = relationship.Id,
            Label = relationship.Name
        };

        private EdgeViewModel NewEdgeViewModel(Relationship relationship)
        {
            var source = this.GraphViewModel.Vertices.FirstOrDefault(v => v.ModelId.Equals(relationship.From.Id));
            var target = this.GraphViewModel.Vertices.FirstOrDefault(v => v.ModelId.Equals(relationship.To.Id));
            return new EdgeViewModel(source, target)
            {
                ModelId = relationship.Id,
                Label = relationship.Name
            };
        }

        private EdgeViewModel ExistingEdgeViewModel(Relationship relationship) => ExistingEdgeViewModel(e => e.ModelId.Equals(relationship.Id));

        private EdgeViewModel ExistingEdgeViewModel(Func<EdgeViewModel, bool> firstOrDefault) => this.GraphViewModel.Edges.FirstOrDefault(firstOrDefault);

        #endregion Manipulate the graph content consistently

        #region Maintain the visualization model

        public IGraphCallback GraphCallback { get; set; }

        public GraphViewModel GraphViewModel { get; } = new GraphViewModel();

        #endregion Maintain the visualization model

        #region IObserver<ChangedMessage<IEntity>> Members

        void IObserver<ChangedMessage<IEntity>>.OnNext(ChangedMessage<IEntity> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Removed:
                    this
                        .GraphViewModel
                        .Vertices
                        .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id))
                        .IfExistsThen(v => this.GraphCallback.Remove(v));
                    break;

                case ChangeTypeValues.Modified:
                    var existingNode = this.GraphViewModel.Vertices
                        .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id));
                    if (existingNode is null)
                    {
                        this.GraphCallback.Add(NewVertexViewModel((Entity)value.Changed));
                    }
                    else
                    {
                        existingNode.Label = value.Changed.Name;
                    }
                    break;
            }
        }

        void IObserver<ChangedMessage<IEntity>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IEntity>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion IObserver<ChangedMessage<IEntity>> Members

        #region IObserver<ChangedMessage<IRelationship>> Members

        void IObserver<ChangedMessage<IRelationship>>.OnNext(ChangedMessage<IRelationship> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Removed:
                    this
                        .GraphViewModel
                        .Edges
                        .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id))
                        .IfExistsThen(e => this.GraphCallback.Remove(e));
                    break;

                case ChangeTypeValues.Modified:
                    var existingEdge = this
                        .GraphViewModel
                        .Edges
                        .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id));

                    if (existingEdge is null)
                    {
                        this.GraphCallback.Add(NewEdgeViewModel((Relationship)value.Changed));
                    }
                    else
                    {
                        // remove edge and add again
                        this
                           .GraphViewModel
                           .Edges
                           .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id))
                           .IfExistsThen(e => this.GraphCallback.Remove(e));

                        var source = this.GraphViewModel.Vertices.FirstOrDefault(v => v.ModelId.Equals(value.Changed.From.Id));
                        var target = this.GraphViewModel.Vertices.FirstOrDefault(v => v.ModelId.Equals(value.Changed.To.Id));
                        this.GraphCallback
                            .Add(new EdgeViewModel(source, target)
                            {
                                ModelId = value.Changed.Id,
                                Label = ((Relationship)value.Changed).Name
                            });
                    }
                    break;
            }
        }

        void IObserver<ChangedMessage<IRelationship>>.OnError(Exception error)

        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IRelationship>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion IObserver<ChangedMessage<IRelationship>> Members
    }
}