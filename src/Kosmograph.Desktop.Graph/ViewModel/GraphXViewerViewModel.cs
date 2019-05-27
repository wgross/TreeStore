using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class GraphXViewerViewModel : IObserver<ChangedMessage<IEntity>>, IObserver<ChangedMessage<IRelationship>>
    {
        private readonly KosmographModel model;
        private readonly IKosmographMessageBus messageBus;
        private readonly IDisposable entitySubscription;
        private readonly IDisposable relationshipSubscription;

        public GraphXViewerViewModel(KosmographModel model, IKosmographMessageBus messageBus)
        {
            this.model = model;
            this.messageBus = messageBus;
            this.entitySubscription = this.messageBus.Entities.Subscribe(this);
            this.relationshipSubscription = this.messageBus.Relationships.Subscribe(this);
        }

        #region Track the shown entities

        public void Show(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (this.GraphCallback is null)
                {
                    this.GraphViewModel.AddVertex(new VertexViewModel
                    {
                        Label = entity.Name,
                        ModelId = entity.Id,
                    });
                }
                else
                {
                    this.GraphCallback.Add(new VertexViewModel
                    {
                        Label = entity.Name,
                        ModelId = entity.Id,
                    });
                }
            }
        }

        #endregion Track the shown entities

        #region Track the shown relationships

        public void Show(IEnumerable<Relationship> relationshipsToShow)
        {
            foreach (var relationship in relationshipsToShow)
            {
                if (this.GraphCallback is null)
                {
                    this.GraphViewModel.AddEdge(NewEdgeViewModel(relationship));
                }
                else this.GraphCallback.Add(NewEdgeViewModel(relationship));
            }
        }

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

        #endregion Track the shown relationships

        #region IObserver<ChangedMessage<IEntity>> Members

        private VertexViewModel NewVertexViewModel(Entity e) => new VertexViewModel
        {
            Label = e.Name,
            ModelId = e.Id,
        };

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

        #region Build graph from tag queries

        public void ShowTag(Tag tag)
        {
            var tagQuery = new TagQuery(this.model, this.messageBus, tag);
            tagQuery.EntityAdded = e => this.AddEntity(e);
            tagQuery.RelationshipAdded = this.AddRelationship;
            tagQuery.StartQuery();
        }

        private void AddRelationship(Relationship obj)
        {
            var existing = ExistingEdgeViewModel(obj);
            if (existing is null)
                this.GraphCallback.Add(this.NewEdgeViewModel(obj, this.AddEntity(obj.From), this.AddEntity(obj.To)));
        }

        private EdgeViewModel ExistingEdgeViewModel(Relationship relationship) => this.GraphViewModel.Edges.FirstOrDefault(e => e.ModelId.Equals(relationship.Id));

        private VertexViewModel AddEntity(Entity added)
        {
            var existing = ExistingVertexViewModel(added);
            if (existing is null)
                this.GraphCallback.Add(existing = NewVertexViewModel(added));
            return existing;
        }

        private VertexViewModel ExistingVertexViewModel(Entity entity) => this.GraphViewModel.Vertices.FirstOrDefault(v => v.ModelId.Equals(entity.Id));

        private EdgeViewModel NewEdgeViewModel(Relationship relationship, VertexViewModel source, VertexViewModel target) => new EdgeViewModel(source, target)
        {
            ModelId = relationship.Id,
            Label = relationship.Name
        };

        #endregion Build graph from tag queries

        void IObserver<ChangedMessage<IRelationship>>.OnError(Exception error)

        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<IRelationship>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion IObserver<ChangedMessage<IRelationship>> Members

        #region Maintain the visualization model

        public IGraphCallback GraphCallback { get; set; }

        public GraphViewModel GraphViewModel { get; } = new GraphViewModel();

        #endregion Maintain the visualization model
    }
}