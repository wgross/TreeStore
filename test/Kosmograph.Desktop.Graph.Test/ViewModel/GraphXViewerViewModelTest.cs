using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Graph.Test.ViewModel
{
    public class GraphXViewerViewModelTest : ViewModelTestBase
    {
        private readonly GraphXViewerViewModel viewModel;
        private readonly Mock<IGraphCallback> graphCallback;

        public GraphXViewerViewModelTest()
        {
            this.graphCallback = this.Mocks.Create<IGraphCallback>();
            this.viewModel = new GraphXViewerViewModel(new KosmographModel(this.Persistence.Object), this.MessageBus);
            this.viewModel.GraphCallback = this.graphCallback.Object;
        }

        [Fact]
        public void GraphXViewerViewModel_subscribes_to_message_bus()
        {
            // ARRANGE

            var messageBusMock = this.Mocks.Create<IKosmographMessageBus>();

            var messageBusTagsMock = this.Mocks.Create<IChangedMessageBus<ITag>>();
            messageBusMock
                .Setup(m => m.Tags)
                .Returns(messageBusTagsMock.Object);

            var messageBusEntitiesMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            messageBusMock
                .Setup(m => m.Entities)
                .Returns(messageBusEntitiesMock.Object);

            var messageBusRelationshipMock = this.Mocks.Create<IChangedMessageBus<IRelationship>>();
            messageBusMock
                .Setup(m => m.Relationships)
                .Returns(messageBusRelationshipMock.Object);

            messageBusTagsMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()))
                .Returns(Mock.Of<IDisposable>());

            messageBusEntitiesMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Returns(Mock.Of<IDisposable>());

            messageBusRelationshipMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new GraphXViewerViewModel(new KosmographModel(this.Persistence.Object), messageBusMock.Object);

            // ASSERT
            // view model observce changes in the model

            messageBusTagsMock.Verify(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()), Times.Once());
            messageBusEntitiesMock.Verify(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()), Times.Once());
            messageBusRelationshipMock.Verify(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()), Times.Once());
        }

        [Fact]
        public void GraphXViewerViewModel_indicates_empty_graph()
        {
            // ACT

            var result = this.viewModel.IsEmpty;

            // ASSERT
            // view model has no items

            Assert.True(result);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_tag_query()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            // prepare tag query finsing one entity and on relationship

            var queryTag = DefaultTag();

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // ACT

            string result = null;
            this.viewModel.PropertyChanged += (s, e) => result = e.PropertyName;
            this.viewModel.ShowTag(queryTag);

            // ASSERT

            // tag query was added to the view model
            Assert.Equal(queryTag.Name, this.viewModel.TagQueries.Single().Name);
            Assert.Equal(3, this.viewModel.GraphViewModel.Vertices.Count());
            Assert.Contains(entity.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Contains(relationship.From.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Contains(relationship.To.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Single(this.viewModel.GraphViewModel.Edges);
            Assert.Contains(relationship.Id, this.viewModel.GraphViewModel.Edges.Select(v => v.ModelId));

            // viewmodel isn't empty any more
            Assert.False(this.viewModel.IsEmpty);
            Assert.Equal(nameof(this.viewModel.IsEmpty), result);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_tag_query()
        {
            // ARRANGE

            // map view model changes to graph changes.

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback
                .Setup(c => c.Remove(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.RemoveVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            this.graphCallback
                .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.RemoveEdge(e));

            // prepare tag query finsing one entity and one relationship

            var queryTag = DefaultTag();

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.viewModel.ShowTag(queryTag);

            // ACT

            string propertyName = null;
            this.viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;
            this.viewModel.RemoveTagCommand.Execute(this.viewModel.TagQueries.Single());

            // ASSERT

            // graph contains no edges or vertices
            Assert.Empty(this.viewModel.TagQueries);
            Assert.Empty(this.viewModel.GraphViewModel.Edges);
            Assert.Empty(this.viewModel.GraphViewModel.Vertices);

            // view model isn't empty any more
            Assert.True(this.viewModel.IsEmpty);
            Assert.Equal(nameof(this.viewModel.IsEmpty), propertyName);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_vertex_on_modified_entity_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            // prepare empty tag query

            var queryTag = DefaultTag();

            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Entity>());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Relationship>());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.viewModel.ShowTag(queryTag);

            // ACT
            // send entity through message bus

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            this.MessageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Single(this.viewModel.GraphViewModel.Vertices);
            Assert.Contains(entity.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Empty(this.viewModel.GraphViewModel.Edges);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_edge_on_on_modified_relationship_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to add relationship
               .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
               .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            // prepare empty tag query

            var queryTag = DefaultTag();

            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Entity>());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Relationship>());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.viewModel.ShowTag(queryTag);

            // ACT
            // send entity through message bus

            var relationship = DefaultRelationship(r => r.Tags.Add(queryTag));
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Single(this.viewModel.GraphViewModel.Edges);
            Assert.Contains(relationship.Id, this.viewModel.GraphViewModel.Edges.Select(v => v.ModelId));
        }

        [Fact]
        public void GraphXViewerViewModel_removes_vertex_on_modified_entity_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to remove entity
                .Setup(c => c.Remove(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.RemoveVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            // prepare tag query finsing one entity and on relationship

            var queryTag = DefaultTag();

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.viewModel.ShowTag(queryTag);

            // ACT
            // send removal through message bus

            entity.Tags.Remove(queryTag);
            this.MessageBus.Entities.Modified(entity);

            // ASSERT

            Assert.DoesNotContain(this.viewModel.GraphViewModel.Vertices, v => v.ModelId == entity.Id);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_vertex_on_removed_entity_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to remove entity
                .Setup(c => c.Remove(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.RemoveVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            // prepare tag query finsing one entity and on relationship

            var queryTag = DefaultTag();

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.viewModel.ShowTag(queryTag);

            // ACT
            // send removal through message bus

            this.MessageBus.Entities.Removed(entity);

            // ASSERT

            Assert.DoesNotContain(this.viewModel.GraphViewModel.Vertices, v => v.ModelId == entity.Id);
        }

        [Fact]
        public void GraphXViewerViewModel_adding_relationship_reuses_existing_entity_from_TagQuery()
        {
            // ARRANGE

            var vertices = new List<Guid>();
            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => vertices.Add(v.ModelId));

            var edges = new List<Guid>();
            this.graphCallback
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(v => edges.Add(v.ModelId));

            var queryTag = DefaultTag();

            var entity = DefaultEntity(e => e.Tags.Add(queryTag));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(entity, DefaultEntity(), r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // entity was edded before
            this.viewModel.GraphViewModel.AddVertex(new VertexViewModel { ModelId = entity.Id });

            // ACT

            this.viewModel.ShowTag(queryTag);

            // ASSERT
            // relationship was added
            Assert.Equal(relationship.Id, edges.Single());
            // three entites were added
            Assert.Equal(new[] { relationship.To.Id }, vertices);
        }

        #region Relationship removal cases: e1(t1)--r1(t2)--e2

        private (Tag, Tag, Entity, Entity, Relationship) PrepareSzenario()
        {
            var t1 = DefaultTag();
            var t2 = DefaultTag();

            var e1 = DefaultEntity(e => e.Tags.Add(t1));
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(t1))
                .Returns(e1.Yield());
            entityRepository
                .Setup(r => r.FindByTag(t2))
                .Returns(Enumerable.Empty<Entity>());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var e2 = DefaultEntity();
            var r1 = DefaultRelationship(e1, e2, r => r.Tags.Add(t2));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(t1))
                .Returns(Enumerable.Empty<Relationship>());
            relationshipRepository
                .Setup(r => r.FindByTag(t2))
                .Returns(r1.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            return (t1, t2, e1, e2, r1);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_r1()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to remove entity
                .Setup(c => c.Remove(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.RemoveVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            this.graphCallback // graph will be called to remove r1
                .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.RemoveEdge(e));

            // persistence

            var (t1, t2, e1, e2, r1) = this.PrepareSzenario();

            this.viewModel.ShowTag(t1);
            this.viewModel.ShowTag(t2);

            // ACT
            // send removal of e1 through message bus

            this.MessageBus.Relationships.Removed(r1);

            // ASSERT
            // r1 removed -> e2 removed; e1 remains

            Assert.Contains(this.viewModel.GraphViewModel.Vertices, v => v.ModelId == e1.Id);
            Assert.DoesNotContain(this.viewModel.GraphViewModel.Vertices, v => v.ModelId == e2.Id);
            Assert.Empty(this.viewModel.GraphViewModel.Edges);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_r1_if_t2_is_removed_from_r1()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Remove(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.RemoveVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            this.graphCallback // graph will be called to remove relationhsip
                .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.RemoveEdge(e));

            // persistence

            var (t1, t2, e1, e2, r1) = this.PrepareSzenario();

            this.viewModel.ShowTag(t1);
            this.viewModel.ShowTag(t2);

            // ACT
            // send removal through message bus

            r1.Tags.Remove(t2);
            this.MessageBus.Relationships.Modified(r1);

            // ASSERT
            // r1 is removed -> e2 removed; e1 remains

            Assert.DoesNotContain(this.viewModel.GraphViewModel.Edges, v => v.ModelId == r1.Id);
            Assert.DoesNotContain(this.viewModel.GraphViewModel.Vertices, v => v.ModelId == e2.Id);
        }

        #endregion Relationship removal cases: e1(t1)--r1(t2)--e2
    }
}