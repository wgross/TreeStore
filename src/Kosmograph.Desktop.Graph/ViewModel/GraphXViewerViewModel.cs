using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class GraphXViewerViewModel : IObserver<ChangedMessage<IEntity>>, IObserver<ChangedMessage<IRelationship>>
    {
        private readonly IKosmographMessageBus messageBus;
        private readonly IDisposable entitySubscription;
        private readonly IDisposable relationshipSubscription;

        public GraphXViewerViewModel(IKosmographMessageBus messageBus)
        {
            this.messageBus = messageBus;
            this.entitySubscription = this.messageBus.Entities.Subscribe(this);
            this.relationshipSubscription = this.messageBus.Relationships.Subscribe(this);
            this.entities.CollectionChanged += this.Entities_CollectionChanged;
        }

        #region Track the shown entities

        private readonly ObservableCollection<Entity> entities = new ObservableCollection<Entity>();

        public IEnumerable<Entity> Entities => entities;

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

        private readonly ObservableCollection<Relationship> relationships = new ObservableCollection<Relationship>();

        public IEnumerable<Relationship> Relationships => relationships;

        public void Show(IEnumerable<Relationship> relationships)
        {
            foreach (var relationship in relationships)
                this.relationships.Add(relationship);
        }

        #endregion Track the shown relationships

        #region Handle changes of model entities

        void IObserver<ChangedMessage<IEntity>>.OnNext(ChangedMessage<IEntity> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Removed:
                    this.entities
                        .FirstOrDefault(e => e.Id.Equals(value.Changed.Id))
                        .IfExistsThen(e => this.entities.Remove(e));
                    break;

                case ChangeTypeValues.Modified:
                    var existingNode = this.GraphViewModel.Vertices
                        .FirstOrDefault(v => v.ModelId.Equals(value.Changed.Id));
                    if (existingNode is null)
                    {
                        this.entities.Add((Entity)value.Changed);
                        //this.GraphCallback.Add(new VertexViewModel
                        //{
                        //    ModelId = value.Changed.Id,
                        //    Label = value.Changed.Name
                        //});
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

        public void OnNext(ChangedMessage<IRelationship> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Removed:
                    this.relationships
                        .FirstOrDefault(e => e.Id.Equals(value.Changed.Id))
                        .IfExistsThen(e => this.relationships.Remove(e));
                    break;
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion Handle changes of model entities

        #region Maintain the visualization model

        public IGraphCallback GraphCallback { get; set; }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            switch (ev.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ev.NewItems.OfType<Entity>().ForEach(e => this.GraphCallback.Add(new VertexViewModel
                    {
                        Label = e.Name,
                        ModelId = e.Id,
                    }));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    ev.OldItems.OfType<Entity>().ForEach(e =>
                    {
                        this.GraphViewModel.Vertices
                            .FirstOrDefault(v => v.ModelId.Equals(e.Id))
                            .IfExistsThen(v => this.GraphCallback.Remove(v));
                    });
                    break;
            }
        }

        public GraphViewModel GraphViewModel { get; } = new GraphViewModel();

        #endregion Maintain the visualization model
    }
}