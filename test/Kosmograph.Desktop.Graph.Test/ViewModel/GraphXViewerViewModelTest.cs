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

            this.viewModel.ShowTag(queryTag);

            // ASSERT

            Assert.Equal(queryTag.Name, this.viewModel.TagQueries.Single().Name);
            Assert.Equal(3, this.viewModel.GraphViewModel.Vertices.Count());
            Assert.Contains(entity.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Contains(relationship.From.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Contains(relationship.To.Id, this.viewModel.GraphViewModel.Vertices.Select(v => v.ModelId));
            Assert.Single(this.viewModel.GraphViewModel.Edges);
            Assert.Contains(relationship.Id, this.viewModel.GraphViewModel.Edges.Select(v => v.ModelId));
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
        public void GraphXViewerViewModel_removes_edge_on_modified_relationship_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            this.graphCallback // graph will be called to remove relationhsip
                .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.RemoveEdge(e));

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

            relationship.Tags.Remove(queryTag);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.DoesNotContain(this.viewModel.GraphViewModel.Edges, v => v.ModelId == relationship.Id);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_edge_on_removed_relationship_message()
        {
            // ARRANGE

            this.graphCallback // graph will be called to add entity
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()))
                .Callback<VertexViewModel>(v => this.viewModel.GraphViewModel.AddVertex(v));

            this.graphCallback // graph will be called to add relationship
                .Setup(c => c.Add(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.AddEdge(e));

            this.graphCallback // graph will be called to remove relationhsip
                .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()))
                .Callback<EdgeViewModel>(e => this.viewModel.GraphViewModel.RemoveEdge(e));

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

            this.MessageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.DoesNotContain(this.viewModel.GraphViewModel.Edges, v => v.ModelId == relationship.Id);
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
    }
}