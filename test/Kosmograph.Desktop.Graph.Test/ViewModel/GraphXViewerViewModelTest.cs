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

            var MessageBusMock = this.Mocks.Create<IKosmographMessageBus>();

            var MessageBusEntitiesMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            MessageBusMock
                .Setup(m => m.Entities)
                .Returns(MessageBusEntitiesMock.Object);

            var MessageBusRelationshipMock = this.Mocks.Create<IChangedMessageBus<IRelationship>>();
            MessageBusMock
                .Setup(m => m.Relationships)
                .Returns(MessageBusRelationshipMock.Object);

            MessageBusEntitiesMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Returns(Mock.Of<IDisposable>());

            MessageBusRelationshipMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new GraphXViewerViewModel(new KosmographModel(this.Persistence.Object), MessageBusMock.Object);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entity_to_show()
        {
            // ARRANGE

            var entity = new Entity();

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            // ACT

            this.viewModel.Show(entity.Yield());
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entity_on_added_message()
        {
            // ARRANGE

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            var entity = new Entity();

            // ACT
            // send removal through message bus
            this.MessageBus.Entities.Modified(entity);

            // ASSERT

            // Assert.Single(this.viewModel.Entities);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_entity_on_delete_message()
        {
            // ARRANGE

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            var entity = new Entity();
            this.viewModel.Show(entity.Yield());

            // ACT
            // send removal through message bus
            this.MessageBus.Entities.Removed(entity);

            // ASSERT

            // Assert.Empty(this.viewModel.Entities);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_relationship_to_show()
        {
            // ARRANGE

            var entity = new Relationship();

            // ACT

            this.viewModel.Show(entity.Yield());

            // ASSERT
        }

        [Fact]
        public void GraphXViewerViewModel_removes_relationship_on_delete_message()
        {
            // ARRANGE

            var relationship = new Relationship();
            this.viewModel.Show(relationship.Yield());

            this.graphCallback
              .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()));

            // ACT

            this.MessageBus.Relationships.Removed(relationship);

            // ASSERT
        }

        [Fact]
        public void GraphXViewerViewModel_updates_relationship_on_modified_message()
        {
            // ARRANGE

            var relationship = new Relationship();
            this.viewModel.Show(relationship.Yield());

            this.graphCallback
              .Setup(c => c.Add(It.IsAny<EdgeViewModel>()));
            this.graphCallback
              .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()));

            // ACT

            this.MessageBus.Relationships.Modified(relationship);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entities_and_relationships_from_TagQuery()
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
            // relationship was added
            Assert.Equal(relationship.Id, edges.Single());
            // three entites were added
            Assert.Equal(new[] { entity.Id, relationship.From.Id, relationship.To.Id }, vertices);
        }

        [Fact]
        public void GraphXViewerViewModel_adding_entity_reuses_existing_entity_from_TagQuery()
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

            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Relationship>());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // entity was edded before
            this.viewModel.GraphViewModel.AddVertex(new VertexViewModel { ModelId = entity.Id });

            // ACT

            this.viewModel.ShowTag(queryTag);

            // ASSERT
            // no additinal entity were added

            Assert.Empty(vertices);
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

        [Fact]
        public void GraphXViewerViewModel_adding_relationship_doesnt_add_existing_edge_TagQuery()
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

            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(Enumerable.Empty<Entity>());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(DefaultEntity(), DefaultEntity(), r => r.Tags.Add(queryTag));
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // relationship was added before

            this.viewModel.GraphViewModel.AddVertex(new VertexViewModel());
            this.viewModel.GraphViewModel.AddVertex(new VertexViewModel());
            this.viewModel.GraphViewModel.AddEdge(new EdgeViewModel(this.viewModel.GraphViewModel.Vertices.First(), this.viewModel.GraphViewModel.Vertices.Last())
            {
                ModelId = relationship.Id
            });

            // ACT

            this.viewModel.ShowTag(queryTag);

            // ASSERT
            // relationship wasn't added
            Assert.Empty(edges);
        }
    }
}